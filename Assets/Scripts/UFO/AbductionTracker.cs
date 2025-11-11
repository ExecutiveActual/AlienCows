using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class AbductionTracker_ : MonoBehaviour
{
    public static AbductionTracker_ Instance { get; private set; }

    [Header("UI References")]
    [Tooltip("TMP text that shows number of cows abducted.")]
    public TMP_Text abductCountText;

    [Tooltip("Image icon to flash when an abduction happens.")]
    public Image abductionIcon;

    [Header("Flicker Settings")]
    [Tooltip("How many times the icon/text flickers when abduction happens.")]
    public int flickerCount = 3;

    [Tooltip("Speed of each flicker (seconds).")]
    public float flickerSpeed = 0.15f;

    [Tooltip("How long both stay visible after flicker ends.")]
    public float holdDuration = 0.5f;

    private int abductedCount;
    private Coroutine flickerRoutine;

    void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (abductCountText) abductCountText.text = "× 0";
        if (abductionIcon) abductionIcon.enabled = false;
    }

    
    public void RegisterAbduction()
    {
        abductedCount++;

        if (abductCountText)
            abductCountText.text = "× " + abductedCount;

        if (flickerRoutine != null)
            StopCoroutine(flickerRoutine);

        flickerRoutine = StartCoroutine(FlickerFeedback());
    }

    private IEnumerator FlickerFeedback()
    {
        if (abductionIcon) abductionIcon.enabled = true;
        if (abductCountText) abductCountText.enabled = true;

        for (int i = 0; i < flickerCount; i++)
        {
            if (abductionIcon) abductionIcon.enabled = !abductionIcon.enabled;
            if (abductCountText) abductCountText.enabled = !abductCountText.enabled;
            yield return new WaitForSeconds(flickerSpeed);
        }

        // Ensure both visible for holdDuration
        if (abductionIcon) abductionIcon.enabled = true;
        if (abductCountText) abductCountText.enabled = true;

        yield return new WaitForSeconds(holdDuration);

        if (abductionIcon) abductionIcon.enabled = false;
        if (abductCountText) abductCountText.enabled = false;
    }
}
