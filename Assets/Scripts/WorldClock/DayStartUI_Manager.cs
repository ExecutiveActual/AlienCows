using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[DisallowMultipleComponent]
public class DayStartUI_Manager: MonoBehaviour
{
    [Header("Required References")]
    public WorldClock_ worldClock;               // your provided world clock script (assign)
    public GameManager_UI gameManagerUI;         // optional - for switching control mode

    [Header("DayStart Panel (can be disabled in editor)")]
    public GameObject dayStartPanel;             // root panel (assign) - CAN BE INACTIVE in editor
    [Tooltip("Panel background inside the dayStartPanel that should remain active when UI shown (optional)")]
    public GameObject panelBackground;           // optional, child of dayStartPanel

    [Header("Text (assign TextMeshPro components inside the panel)")]
    public TMP_Text titleText;                   // big heading
    public TMP_Text cowsText;
    public TMP_Text productionText;
    public TMP_Text shopText;
    public TMP_Text moneyText;

    [Header("Icons (objects to enable/disable)")]
    public GameObject cowBulletIcon;
    public GameObject productionBulletIcon;
    public GameObject shopBulletIcon;
    public GameObject cowIcon;

    [Header("Continue Button (assign from panel)")]
    public Button continueButton;

    [Header("Values")]
    public float productionPerCow = 30f;
    public float moneyAmount = 0f;

    [Header("Typewriter / Timing")]
    public float openingDelay = 0f;              // delay after day start before opening (seconds, realtime)
    public float typeSpeed = 0.04f;              // seconds per character (realtime)
    public float betweenLinesDelay = 0.25f;      // realtime

    // internal state
    int lastShownNight = -1;
    bool runningSequence = false;

    void Reset()
    {
        // sensible defaults
        productionPerCow = 30f;
        typeSpeed = 0.04f;
        betweenLinesDelay = 0.25f;
    }

    void Awake()
    {
        if (dayStartPanel == null)
        {
            Debug.LogError("[DayStartUI_Controller] dayStartPanel is not assigned. Attach the component to an active object and assign your DayStart panel.");
        }

        // Make sure panel is hidden initially. We allow it to be inactive in editor; if it's active, hide it.
        if (dayStartPanel != null && dayStartPanel.activeSelf)
            dayStartPanel.SetActive(false);
    }

    void Update()
    {
        // Safety: require a worldClock reference
        if (worldClock == null || runningSequence)
            return;

        // Check for the exact moment the world clock enters InGame_Day for a new night
        if (worldClock.currentSession == WorldClock_.TimeSession.InGame_Day)
        {
            int night = worldClock.CurrentNight;
            if (night > 0 && night != lastShownNight)
            {
                lastShownNight = night;
                StartCoroutine(TriggerOpenSequence(night));
            }
        }
    }

    IEnumerator TriggerOpenSequence(int night)
    {
        runningSequence = true;

        // Wait opening delay in realtime (so it doesn't get affected by Time.timeScale)
        if (openingDelay > 0f)
            yield return new WaitForSecondsRealtime(openingDelay);

        // Activate the panel root (safe regardless of prior active state)
        if (dayStartPanel != null && !dayStartPanel.activeSelf)
            dayStartPanel.SetActive(true);

        // Optionally ensure panel background is active
        if (panelBackground != null)
            panelBackground.SetActive(true);

        // Freeze game time (full freeze)
        Time.timeScale = 0f;

        // Tell your GameManager to switch control mode to UI if assigned
        gameManagerUI?.UE_OnSwitchControlMode_UI?.Invoke();

        // Prepare initial hidden state for all UI pieces
        PrepareInitialState();

        // Play opening sound if assigned (play on unscaled time)
        if (Application.isPlaying && continueButton != null)
        {
            // nothing special here, play through assigned AudioSource if you want
        }

        // Update dynamic text values
        UpdateMoneyText();
        UpdateCowAndProductionTexts();

        // Run the typed sequence (use realtime waits so timeScale=0 doesn't block)
        yield return StartCoroutine(UiSequenceRoutine(night));

        runningSequence = false;
    }

    void PrepareInitialState()
    {
        if (titleText != null) { titleText.text = ""; titleText.gameObject.SetActive(false); }
        if (cowsText != null) { cowsText.text = ""; cowsText.gameObject.SetActive(false); }
        if (productionText != null) { productionText.text = ""; productionText.gameObject.SetActive(false); }
        if (shopText != null) { shopText.text = ""; shopText.gameObject.SetActive(false); }
        if (moneyText != null) { /* leave money visible if you want; script will set it */ moneyText.gameObject.SetActive(true); }

        if (cowBulletIcon != null) cowBulletIcon.SetActive(false);
        if (productionBulletIcon != null) productionBulletIcon.SetActive(false);
        if (shopBulletIcon != null) shopBulletIcon.SetActive(false);
        if (cowIcon != null) cowIcon.SetActive(false);

        if (continueButton != null) continueButton.gameObject.SetActive(false);
    }

    IEnumerator UiSequenceRoutine(int night)
    {
        // 1) Heading
        string heading = $"Day {night} has started";
        if (titleText != null)
        {
            titleText.gameObject.SetActive(true);
            yield return StartCoroutine(TypeTextRealtime(titleText, heading, typeSpeed));
            yield return new WaitForSecondsRealtime(betweenLinesDelay);
        }

        // 2) Cows row
        if (cowBulletIcon != null) cowBulletIcon.SetActive(true);
        if (cowIcon != null) cowIcon.SetActive(true);
        UpdateCowAndProductionTexts(); // refresh counts
        if (cowsText != null)
        {
            cowsText.gameObject.SetActive(true);
            yield return StartCoroutine(TypeTextRealtime(cowsText, cowsText.text, typeSpeed));
            yield return new WaitForSecondsRealtime(betweenLinesDelay);
        }

        // 3) Production row
        if (productionBulletIcon != null) productionBulletIcon.SetActive(true);
        if (productionText != null)
        {
            productionText.gameObject.SetActive(true);
            yield return StartCoroutine(TypeTextRealtime(productionText, productionText.text, typeSpeed));
            yield return new WaitForSecondsRealtime(betweenLinesDelay);
        }

        // 4) Shop row
        if (shopBulletIcon != null) shopBulletIcon.SetActive(true);
        if (shopText != null)
        {
            shopText.gameObject.SetActive(true);
            yield return StartCoroutine(TypeTextRealtime(shopText, shopText.text, typeSpeed));
            yield return new WaitForSecondsRealtime(betweenLinesDelay);
        }

        // 5) Show Continue button and hook it
        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(true);
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(OnContinuePressed);
        }
    }

    IEnumerator TypeTextRealtime(TMP_Text tmp, string full, float speed)
    {
        if (tmp == null) yield break;
        tmp.text = "";
        for (int i = 0; i < full.Length; i++)
        {
            tmp.text += full[i];
            yield return new WaitForSecondsRealtime(Mathf.Max(0.001f, speed));
        }
    }

    void UpdateMoneyText()
    {
        if (moneyText != null)
            moneyText.text = $"You have {moneyAmount}$";
    }

    void UpdateCowAndProductionTexts()
    {
        int cows = CountCowsInScene();
        if (cowsText != null) cowsText.text = $"You have {cows} cows left";
        if (productionText != null) productionText.text = $"Each cow will produce {productionPerCow}$ each day\nTotal: {cows * productionPerCow}$";
        if (shopText != null && string.IsNullOrEmpty(shopText.text)) shopText.text = "You can buy more cows from the shop";
    }

    int CountCowsInScene()
    {
        int count = 0;
        try { count = GameObject.FindGameObjectsWithTag("Cow").Length; } catch { }
        if (count == 0)
        {
            try { count = GameObject.FindGameObjectsWithTag("cow").Length; } catch { }
        }
        return count;
    }

    void OnContinuePressed()
    {
        // Unfreeze
        Time.timeScale = 1f;

        // Switch back to player control
        gameManagerUI?.UE_OnSwitchControlMode_Player?.Invoke();

        // Hide the panel (we hide the whole root)
        if (dayStartPanel != null) dayStartPanel.SetActive(false);

        // Unhook the button callback to avoid duplicates
        if (continueButton != null)
            continueButton.onClick.RemoveAllListeners();
    }

    // Optional: public method to open manually (useful for debugging)
    public void OpenNowManually()
    {
        StartCoroutine(TriggerOpenSequence((worldClock != null) ? Mathf.Max(1, worldClock.CurrentNight) : 1));
    }

    // Optional: public setter for money amount at runtime
    public void SetMoneyAmount(float amount)
    {
        moneyAmount = amount;
        UpdateMoneyText();
    }
}
