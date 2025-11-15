using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;

/// <summary>
/// UFOUI
/// - Place on an empty GameObject and assign a reference to a particular UFOController (or auto-find when spawned).
/// - Updates TMP texts for short ephemeral messages (typewriter style), and persistent scores like money and counts.
/// - Default state: startTextsDisabled (inspector) = true (so UI texts are hidden).
/// - Uses UnityEvents for hooking into the UFO lifecycle.
/// </summary>
public class UFOUI : MonoBehaviour
{
    [Header("UFO Reference (assign or let runtime set)")]
    [Tooltip("Optional: assign the UFOController instance this UI should listen to. If null, the UI will try to find one in the parent or children.")]
    public UFOController ufoController;

    [Header("Text Elements (TMP)")]
    [Tooltip("Persistent money text: +50$ style (editable increment value).")]
    public TMP_Text moneyText;

    [Tooltip("Persistent UFO Down count text (e.g., x1, x2).")]
    public TMP_Text ufoDownCountText;

    [Tooltip("Persistent UFO Fleed count text (e.g., x1).")]
    public TMP_Text ufoFledCountText;

    [Header("Short ephemeral message (typewriter)")]
    [Tooltip("Short message that appears for a few seconds and then disappears (typewriter in/out).")]
    public TMP_Text ephemeralMessageText;

    [Header("Typewriter Settings")]
    [Tooltip("Time per character for typewriter in.")]
    public float typeInSpeed = 0.03f;

    [Tooltip("Time per character for typewriter out (deleting).")]
    public float typeOutSpeed = 0.02f;

    [Tooltip("How long the ephemeral message stays before the delete animation starts.")]
    public float ephemeralHoldTime = 1.2f;

    [Header("Defaults")]
    [Tooltip("Is the UI initially disabled? (recommended true).")]
    public bool startTextsDisabled = true;

    [Header("Scoring Values")]
    [Tooltip("Money gained per ufo destroyed.")]
    public int moneyPerUfo = 50;

    [Tooltip("Delay before showing the results panel when all UFOs are gone (seconds).")]
    public float resultsPanelDelay = 1.2f;

    [Header("Optional Results Panel")]
    public GameObject resultsPanel;
    public TMP_Text resultsTotalMoney;
    public TMP_Text resultsUFOFled;
    public TMP_Text resultsUFODown;
    public TMP_Text resultsCowsLost;

    // internal counters
    private int totalMoney = 0;
    private int ufosDown = 0;
    private int ufosFled = 0;
    private int cowsLost = 0;

    private void Start()
    {
        if (ufoController == null)
        {
            ufoController = GetComponentInParent<UFOController>() ?? GetComponentInChildren<UFOController>();
        }

        if (startTextsDisabled)
        {
            if (ephemeralMessageText != null) ephemeralMessageText.gameObject.SetActive(false);
            if (moneyText != null) moneyText.gameObject.SetActive(false);
            if (ufoDownCountText != null) ufoDownCountText.gameObject.SetActive(false);
            if (ufoFledCountText != null) ufoFledCountText.gameObject.SetActive(false);
        }

        // Hook into controller events if present
        if (ufoController != null)
        {
            ufoController.onLifecycleEnded.AddListener(OnUfoLifecycleEnded);
            ufoController.onCowReleased.AddListener(OnCowReleased);
            // Additional wiring (like health events) can be done by visuals from UFOHealth directly if necessary.
        }
    }

    // Public calls used by other scripts to update UI
    public void NotifyUfoDestroyed(int moneyAmount)
    {
        ufosDown++;
        totalMoney += moneyAmount;
        UpdatePersistentTexts();

        // ephemeral message: "1x UFO Down" typewrite
        TriggerEphemeralMessage($"{ufosDown}x UFO DOWN");
    }

    public void NotifyUfoFled()
    {
        ufosFled++;
        UpdatePersistentTexts();
        TriggerEphemeralMessage($"{ufosFled}x UFO FLED");
    }

    public void NotifyMoneyGained(int amount)
    {
        totalMoney += amount;
        UpdatePersistentTexts();
        // no ephemeral typewrite for money, per your spec: money text just updates like +$50
        if (moneyText != null)
        {
            moneyText.gameObject.SetActive(true);
            moneyText.text = $"+{totalMoney}$";
        }
    }

    private void UpdatePersistentTexts()
    {
        if (ufoDownCountText != null)
        {
            ufoDownCountText.gameObject.SetActive(true);
            ufoDownCountText.text = $"x{ufosDown}";
        }

        if (ufoFledCountText != null)
        {
            ufoFledCountText.gameObject.SetActive(true);
            ufoFledCountText.text = $"x{ufosFled}";
        }

        if (moneyText != null)
        {
            moneyText.gameObject.SetActive(true);
            moneyText.text = $"+{totalMoney}$";
        }
    }

    private void TriggerEphemeralMessage(string message)
    {
        if (ephemeralMessageText == null) return;
        StopAllCoroutines();
        StartCoroutine(TypewriterEphemeralRoutine(message));
    }

    private IEnumerator TypewriterEphemeralRoutine(string message)
    {
        ephemeralMessageText.gameObject.SetActive(true);
        ephemeralMessageText.text = "";
        int total = message.Length;
        for (int i = 0; i < total; i++)
        {
            ephemeralMessageText.text += message[i];
            yield return new WaitForSeconds(typeInSpeed);
        }

        yield return new WaitForSeconds(ephemeralHoldTime);

        // delete out
        for (int i = total; i > 0; i--)
        {
            ephemeralMessageText.text = ephemeralMessageText.text.Substring(0, i - 1);
            yield return new WaitForSeconds(typeOutSpeed);
        }

        ephemeralMessageText.gameObject.SetActive(false);
    }

    // Called by controller when a cow is released (fell or destroyed)
    private void OnCowReleased(Transform cow)
    {
        if (cow == null)
        {
            cowsLost++; // if cow destroyed while abducting counts as lost? adjust as needed
        }
    }

    private void OnUfoLifecycleEnded()
    {
        // If all ufos are gone from the scene, optionally show results panel after delay.
        StartCoroutine(ShowResultsIfAllGoneRoutine());
    }

    private IEnumerator ShowResultsIfAllGoneRoutine()
    {
        yield return new WaitForSeconds(resultsPanelDelay);

        // The logic to determine "all ufos are gone" depends on your spawner; for now, show panel if assigned
        if (resultsPanel != null)
        {
            resultsPanel.SetActive(true);
            if (resultsTotalMoney != null) resultsTotalMoney.text = $"+{totalMoney}$";
            if (resultsUFOFled != null) resultsUFOFled.text = ufosFled.ToString();
            if (resultsUFODown != null) resultsUFODown.text = ufosDown.ToString();
            if (resultsCowsLost != null) resultsCowsLost.text = cowsLost.ToString();
        }
    }
}
