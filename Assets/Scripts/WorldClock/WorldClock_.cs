using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// WorldClock_ — night/day cycles + integrated DayStart UI.
/// Includes: openingDelay, freeze-on-open, gameManager UI event invocations,
/// opening sound, realtime UI sequencing, IsNight(), SmoothSkyTransition(), and robust null checks.
/// Replace your current WorldClock_.cs with this file and assign inspector references.
/// </summary>
public class WorldClock_ : MonoBehaviour
{
    //===========================//
    //      ENUM DEFINITIONS     //
    //===========================//
    public enum TimeSession { PreGame, InGame_Night, InGame_Day, Idle }

    public enum NotificationSoundMode
    {
        PlayAtStart,
        PlayAtMid,
        PlayAtEnd
    }

    //===========================//
    //        TIME CONFIG        //
    //===========================//
    [Header("Time Durations (Seconds)")]
    public float preGameDelayAfterDelete = 0.5f;
    public float nightDuration = 60f;
    public float dayDuration = 60f;
    public int totalNights = 15;

    //===========================//
    //        UI SETTINGS        //
    //===========================//
    [Header("Night Notifications")]
    public List<string> nightNotificationMessages = new List<string>();
    public TMP_Text notificationText;

    [Range(0.001f, 0.2f)] public float typeSpeed = 0.04f;
    [Range(0.001f, 0.2f)] public float deleteSpeed = 0.02f;
    public float messageHoldTime = 1.5f;

    //===========================//
    //     INDICATOR IMAGE       //
    //===========================//
    [Header("Typewriter Indicator Image (Fades In/Out)")]
    public GameObject typeIndicatorImage;
    public float indicatorFadeDuration = 0.35f;

    //===========================//
    //     AUDIO SETTINGS        //
    //===========================//
    [Header("Notification Audio")]
    public AudioSource notificationAudioSource;
    public AudioClip notificationClip;
    [Range(0f, 1f)] public float notificationVolume = 1f;
    public NotificationSoundMode soundMode = NotificationSoundMode.PlayAtStart;

    //===========================//
    //     SKY & LIGHTING        //
    //===========================//
    [Header("Skybox & Lighting")]
    public Material skyboxMaterial;
    public Light sunLight;

    [Range(0f, 1f)] public float nightAtmosphereThickness = 0.15f;
    [Range(0f, 1f)] public float dayAtmosphereThickness = 1f;

    public float nightSunIntensity = 0.1f;
    public float daySunIntensity = 1.2f;

    [Range(0.05f, 2f)] public float transitionSpeed = 0.3f;

    //===========================//
    //        STATE DATA         //
    //===========================//
    public TimeSession currentSession { get; private set; }
    public int CurrentNight { get; private set; } = 0;
    public bool IsGameComplete { get; private set; } = false;

    private Coroutine clockRoutine;

    //=========================================================//
    //                 DAY UI (INTEGRATED)                     //
    //=========================================================//
    [Header("Day UI Root (assign panel - can be inactive)")]
    public GameObject dayUIPanel;

    [Header("Day UI Texts (assign inside panel)")]
    public TMP_Text dayTitleText;
    public TMP_Text cowsText;
    public TMP_Text productionText;
    public TMP_Text shopText;
    public TMP_Text moneyText;

    [Header("Day UI Icons")]
    public GameObject cowBulletIcon;
    public GameObject prodBulletIcon;
    public GameObject shopBulletIcon;
    public GameObject cowIcon;

    [Header("Continue Button (assign)")]
    public Button continueButton;

    [Header("Day UI Values")]
    public float productionPerCow = 30f;
    public float moneyAmount = 0f;

    [Header("Day UI Animation Settings")]
    public float openingDelay = 0f;           // WAIT before opening the panel (unscaled)
    public float dayUITypeSpeed = 0.04f;     // per-char (realtime)
    public float betweenLinesDelay = 0.25f;  // realtime

    [Header("Day UI Audio (optional)")]
    public AudioSource dayUIAudioSource;
    public AudioClip dayUIOpenClip;
    [Range(0f, 1f)] public float dayUIOpenVolume = 1f;

    [Header("Game Manager UI (optional)")]
    public GameManager_UI gameManagerUI; // to invoke UE_OnSwitchControlMode_UI / _Player

    // internal
    private Coroutine runningDayUICoroutine = null;

    //======================================================//
    //                      UNITY FLOW                      //
    //======================================================//
    private void Start()
    {
        // safe defaults and restore visuals
        if (skyboxMaterial != null)
            RenderSettings.skybox = skyboxMaterial;

        if (notificationText != null)
        {
            Color c = notificationText.color;
            c.a = 0f;
            notificationText.color = c;
            notificationText.text = "";
        }

        if (typeIndicatorImage != null)
            typeIndicatorImage.SetActive(false);

        if (dayUIPanel != null)
            dayUIPanel.SetActive(false); // keep panel hidden initially

        StartClock();
    }

    //======================================================//
    //                   PUBLIC / API                       //
    //======================================================//
    public void StartClock()
    {
        if (clockRoutine != null)
            StopCoroutine(clockRoutine);

        IsGameComplete = false;
        CurrentNight = 0;
        clockRoutine = StartCoroutine(RunNightCycles());
    }

    public void StopClock()
    {
        if (clockRoutine != null)
            StopCoroutine(clockRoutine);

        currentSession = TimeSession.Idle;
        Debug.Log("[WorldClock_] Clock stopped manually.");
    }

    // Compatibility method used by other scripts (e.g., Cow.cs)
    public bool IsNight()
    {
        return currentSession == TimeSession.InGame_Night;
    }

    // Keep if other systems call it
    public void TriggerPostGame()
    {
        Debug.Log("[WorldClock_] PostGame triggered externally.");
    }

    //======================================================//
    //                  CORE NIGHT/DAY CYCLE                //
    //======================================================//
    private IEnumerator RunNightCycles()
    {
        for (int i = 0; i < totalNights; i++)
        {
            CurrentNight = i + 1;

            yield return StartCoroutine(PreGameSession(CurrentNight));
            yield return StartCoroutine(BeginNight());
            yield return StartCoroutine(BeginDay());

            if (CurrentNight >= totalNights)
                break;
        }

        IsGameComplete = true;
        currentSession = TimeSession.Idle;
        Debug.Log("[WorldClock_] All nights completed. Awaiting PostGame trigger.");
    }

    private IEnumerator PreGameSession(int nightIndex)
    {
        currentSession = TimeSession.PreGame;

        string message = $"Night {nightIndex} - Stay Alert";

        if (nightNotificationMessages != null &&
            nightNotificationMessages.Count >= nightIndex &&
            !string.IsNullOrEmpty(nightNotificationMessages[nightIndex - 1]))
        {
            message = nightNotificationMessages[nightIndex - 1];
        }

        Debug.Log($"[WorldClock_] Pre-Game for Night {nightIndex}: \"{message}\"");

        if (notificationText != null)
        {
            yield return StartCoroutine(TypewriterIn(message));
            yield return new WaitForSeconds(messageHoldTime);
            yield return StartCoroutine(TypewriterOut());
            yield return new WaitForSeconds(preGameDelayAfterDelete);
        }
        else
        {
            yield return new WaitForSeconds(preGameDelayAfterDelete);
        }
    }

    private IEnumerator BeginNight()
    {
        currentSession = TimeSession.InGame_Night;
        Debug.Log($"[WorldClock_] Night {CurrentNight} Started");

        yield return StartCoroutine(SmoothSkyTransition(nightAtmosphereThickness, nightSunIntensity));
        yield return new WaitForSeconds(nightDuration);
    }

    private IEnumerator BeginDay()
    {
        currentSession = TimeSession.InGame_Day;
        Debug.Log($"[WorldClock_] Day {CurrentNight} Started");

        // Start Day UI routine from here — this script is active so coroutine-safe.
        if (runningDayUICoroutine != null)
            StopCoroutine(runningDayUICoroutine);

        runningDayUICoroutine = StartCoroutine(RunDayUI());

        yield return StartCoroutine(SmoothSkyTransition(dayAtmosphereThickness, daySunIntensity));
        yield return new WaitForSeconds(dayDuration);
    }

    //======================================================//
    //                SMOOTH SKY TRANSITION                 //
    //======================================================//
    private IEnumerator SmoothSkyTransition(float targetAtmosphere, float targetIntensity)
    {
        if (skyboxMaterial == null || sunLight == null)
            yield break;

        float startAtmos = 0f;
        bool hasAtmos = true;

        try { startAtmos = skyboxMaterial.GetFloat("_AtmosphereThickness"); }
        catch { hasAtmos = false; }

        float startIntensity = sunLight.intensity;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * transitionSpeed;
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            if (hasAtmos)
                skyboxMaterial.SetFloat("_AtmosphereThickness", Mathf.Lerp(startAtmos, targetAtmosphere, smoothT));

            sunLight.intensity = Mathf.Lerp(startIntensity, targetIntensity, smoothT);
            yield return null;
        }
    }

    //======================================================//
    //                 TYPEWRITER + INDICATOR                //
    //======================================================//
    private IEnumerator FadeIndicator(bool fadeIn)
    {
        if (typeIndicatorImage == null)
            yield break;

        CanvasGroup cg = typeIndicatorImage.GetComponent<CanvasGroup>();
        if (cg == null) cg = typeIndicatorImage.AddComponent<CanvasGroup>();

        typeIndicatorImage.SetActive(true);

        float start = fadeIn ? 0f : 1f;
        float end = fadeIn ? 1f : 0f;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.001f, indicatorFadeDuration);
            cg.alpha = Mathf.Lerp(start, end, t);
            yield return null;
        }

        cg.alpha = end;
        if (!fadeIn) typeIndicatorImage.SetActive(false);
    }

    private IEnumerator TypewriterIn(string message)
    {
        StartCoroutine(FadeIndicator(true));
        SetNotificationAlpha(1f);

        if (notificationText != null)
            notificationText.text = "";

        bool soundPlayed = false;
        for (int i = 0; i < message.Length; i++)
        {
            if (notificationText != null) notificationText.text += message[i];

            if (!soundPlayed && notificationAudioSource != null && notificationClip != null)
            {
                bool shouldPlay =
                    (soundMode == NotificationSoundMode.PlayAtStart && i == 0) ||
                    (soundMode == NotificationSoundMode.PlayAtMid && i == message.Length / 2) ||
                    (soundMode == NotificationSoundMode.PlayAtEnd && i == message.Length - 1);

                if (shouldPlay)
                {
                    notificationAudioSource.PlayOneShot(notificationClip, notificationVolume);
                    soundPlayed = true;
                }
            }

            yield return new WaitForSeconds(typeSpeed);
        }
    }

    private IEnumerator TypewriterOut()
    {
        if (notificationText == null) yield break;

        while (notificationText.text.Length > 0)
        {
            notificationText.text = notificationText.text.Substring(0, notificationText.text.Length - 1);
            yield return new WaitForSeconds(deleteSpeed);
        }

        StartCoroutine(FadeIndicator(false));
        StartCoroutine(FadeOutNotification(0.2f));
        yield return new WaitForSeconds(0.2f);
    }

    private IEnumerator FadeOutNotification(float duration)
    {
        if (notificationText == null) yield break;

        Color c = notificationText.color;
        float startAlpha = c.a;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.0001f, duration);
            c.a = Mathf.Lerp(startAlpha, 0f, t);
            notificationText.color = c;
            yield return null;
        }

        if (notificationText != null) notificationText.text = "";
    }

    private void SetNotificationAlpha(float alpha)
    {
        if (notificationText == null) return;
        Color c = notificationText.color;
        c.a = alpha;
        notificationText.color = c;
    }

    //======================================================//
    //                    DAY UI SEQUENCE                   //
    //======================================================//
    private IEnumerator RunDayUI()
    {
        // Safety: if no panel, skip gracefully
        if (dayUIPanel == null)
        {
            Debug.LogWarning("[WorldClock_] dayUIPanel not assigned — skipping Day UI.");
            yield break;
        }

        // 1) Optional opening delay (unscaled so game still runs if un-frozen)
        if (openingDelay > 0f)
            yield return new WaitForSecondsRealtime(openingDelay);

        // 2) Freeze the world and switch control to UI
        Time.timeScale = 0f;
        if (gameManagerUI != null)
            gameManagerUI.UE_OnSwitchControlMode_UI?.Invoke();

        // 3) Activate panel (safe even if it was inactive in editor)
        dayUIPanel.SetActive(true);

        // 4) Prepare hidden state
        PrepareDayUIHiddenState();

        // 5) Fill values (fresh)
        if (moneyText != null) moneyText.text = $"You have {moneyAmount}$";
        int cows = CountCows();
        if (cowsText != null) cowsText.text = $"You have {cows} cows left";
        if (productionText != null) productionText.text = $"Each cow produces {productionPerCow}$\nTotal: {cows * productionPerCow}$";
        if (shopText != null && string.IsNullOrEmpty(shopText.text)) shopText.text = "You can buy more cows from the shop";

        // 6) Play opening sound (optional)
        if (dayUIAudioSource != null && dayUIOpenClip != null)
            dayUIAudioSource.PlayOneShot(dayUIOpenClip, dayUIOpenVolume);

        // 7) Typewriter sequence (realtime so the UI advances while timeScale==0)
        string heading = $"Day {CurrentNight} has started";

        if (dayTitleText != null)
        {
            dayTitleText.gameObject.SetActive(true);
            yield return StartCoroutine(TypeWriterRealtime(dayTitleText, heading));
            yield return new WaitForSecondsRealtime(betweenLinesDelay);
        }

        if (cowBulletIcon != null) cowBulletIcon.SetActive(true);
        if (cowIcon != null) cowIcon.SetActive(true);
        if (cowsText != null)
        {
            cowsText.gameObject.SetActive(true);
            yield return StartCoroutine(TypeWriterRealtime(cowsText, cowsText.text));
            yield return new WaitForSecondsRealtime(betweenLinesDelay);
        }

        if (prodBulletIcon != null) prodBulletIcon.SetActive(true);
        if (productionText != null)
        {
            productionText.gameObject.SetActive(true);
            yield return StartCoroutine(TypeWriterRealtime(productionText, productionText.text));
            yield return new WaitForSecondsRealtime(betweenLinesDelay);
        }

        if (shopBulletIcon != null) shopBulletIcon.SetActive(true);
        if (shopText != null)
        {
            shopText.gameObject.SetActive(true);
            yield return StartCoroutine(TypeWriterRealtime(shopText, shopText.text));
            yield return new WaitForSecondsRealtime(betweenLinesDelay);
        }

        if (continueButton != null)
        {
            continueButton.gameObject.SetActive(true);
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(OnContinuePressed);
        }

        runningDayUICoroutine = null;
    }

    private void PrepareDayUIHiddenState()
    {
        if (dayTitleText != null) { dayTitleText.text = ""; dayTitleText.gameObject.SetActive(false); }
        if (cowsText != null) { cowsText.text = ""; cowsText.gameObject.SetActive(false); }
        if (productionText != null) { productionText.text = ""; productionText.gameObject.SetActive(false); }
        if (shopText != null) { shopText.text = ""; shopText.gameObject.SetActive(false); }
        if (moneyText != null) moneyText.gameObject.SetActive(true); // keep money visible if present

        if (cowBulletIcon != null) cowBulletIcon.SetActive(false);
        if (prodBulletIcon != null) prodBulletIcon.SetActive(false);
        if (shopBulletIcon != null) shopBulletIcon.SetActive(false);
        if (cowIcon != null) cowIcon.SetActive(false);

        if (continueButton != null) continueButton.gameObject.SetActive(false);
    }

    private IEnumerator TypeWriterRealtime(TMP_Text txt, string full)
    {
        if (txt == null) yield break;
        txt.text = "";
        for (int i = 0; i < full.Length; i++)
        {
            txt.text += full[i];
            yield return new WaitForSecondsRealtime(Mathf.Max(0.001f, dayUITypeSpeed));
        }
    }

    private void OnContinuePressed()
    {
        // Unfreeze and switch back to player mode
        Time.timeScale = 1f;

        if (gameManagerUI != null)
            gameManagerUI.UE_OnSwitchControlMode_Player?.Invoke();

        // hide panel
        if (dayUIPanel != null)
            dayUIPanel.SetActive(false);

        // cleanup button
        if (continueButton != null)
            continueButton.onClick.RemoveAllListeners();
    }

    //======================================================//
    //                    UTILITIES                         //
    //======================================================//
    private int CountCows()
    {
        int count = 0;
        try { count = GameObject.FindGameObjectsWithTag("Cow").Length; } catch { }
        if (count == 0)
        {
            try { count = GameObject.FindGameObjectsWithTag("cow").Length; } catch { }
        }
        return count;
    }
}
