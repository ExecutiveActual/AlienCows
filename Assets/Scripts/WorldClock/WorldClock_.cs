using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Industrial-grade world clock that cycles through multiple nights and days.
/// Each night is preceded by a PreGame session that displays a custom notification
/// with typewriter-in and delete-out animations, plus an optional notification SFX.
/// Now includes fade-in/out image indicator.
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

    [Range(0.001f, 0.2f)]
    public float typeSpeed = 0.04f;

    [Range(0.001f, 0.2f)]
    public float deleteSpeed = 0.02f;

    public float messageHoldTime = 1.5f;


    //===========================//
    //     INDICATOR IMAGE       //
    //===========================//

    [Header("Typewriter Indicator Image (Fades In/Out)")]
    public GameObject typeIndicatorImage;
    public float indicatorFadeDuration = 0.35f; // public fade control


    //===========================//
    //     AUDIO (NOTIFICATION)  //
    //===========================//

    [Header("Notification Audio")]
    public AudioSource notificationAudioSource;
    public AudioClip notificationClip;
    [Range(0f, 1f)] public float notificationVolume = 1f;

    public NotificationSoundMode soundMode = NotificationSoundMode.PlayAtStart;


    //===========================//
    //     SKY & LIGHT SETTINGS  //
    //===========================//

    [Header("Skybox & Lighting")]
    public Material skyboxMaterial;
    public Light sunLight;

    [Range(0f, 1f)] public float nightAtmosphereThickness = 0.15f;
    [Range(0f, 1f)] public float dayAtmosphereThickness = 1f;

    public float nightSunIntensity = 0.1f;
    public float daySunIntensity = 1.2f;

    [Range(0.05f, 2f)]
    public float transitionSpeed = 0.3f;


    //===========================//
    //        STATE DATA         //
    //===========================//

    public TimeSession currentSession { get; private set; }
    public int CurrentNight { get; private set; } = 0;
    public bool IsGameComplete { get; private set; } = false;

    private Coroutine clockRoutine;


    //===========================//
    //        UNITY FLOW         //
    //===========================//

    private void Start()
    {
        if (skyboxMaterial != null)
            RenderSettings.skybox = skyboxMaterial;

        if (notificationText != null)
        {
            var c = notificationText.color;
            c.a = 0f;
            notificationText.color = c;
            notificationText.text = "";
        }

        if (typeIndicatorImage != null)
            typeIndicatorImage.SetActive(false);

        StartClock();
    }


    //===========================//
    //        PUBLIC API         //
    //===========================//

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

    public bool IsNight() => currentSession == TimeSession.InGame_Night;

    public void TriggerPostGame()
    {
        Debug.Log("[WorldClock_] PostGame triggered externally.");
    }


    //===========================//
    //       CORE ROUTINE        //
    //===========================//

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


    //===========================//
    //     SESSION HANDLERS      //
    //===========================//

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
        yield return StartCoroutine(SmoothSkyTransition(dayAtmosphereThickness, daySunIntensity));
        yield return new WaitForSeconds(dayDuration);
    }


    //===========================//
    //    VISUAL TRANSITIONS     //
    //===========================//

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


    //===========================//
    //    IMAGE FADE HELPERS     //
    //===========================//

    private IEnumerator FadeIndicator(bool fadeIn)
    {
        if (typeIndicatorImage == null)
            yield break;

        CanvasGroup cg = typeIndicatorImage.GetComponent<CanvasGroup>();

        if (cg == null)
        {
            cg = typeIndicatorImage.AddComponent<CanvasGroup>();
        }

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

        if (!fadeIn)
            typeIndicatorImage.SetActive(false);
    }


    //===========================//
    //       TYPEWRITER UI       //
    //===========================//

    private IEnumerator TypewriterIn(string message)
    {
        StartCoroutine(FadeIndicator(true)); // fade-in indicator

        SetNotificationAlpha(1f);
        notificationText.text = "";

        int totalChars = message.Length;
        bool soundPlayed = false;

        for (int i = 0; i < totalChars; i++)
        {
            notificationText.text += message[i];

            // SOUND
            if (!soundPlayed && notificationAudioSource != null && notificationClip != null)
            {
                bool shouldPlay =
                    (soundMode == NotificationSoundMode.PlayAtStart && i == 0) ||
                    (soundMode == NotificationSoundMode.PlayAtMid && i == totalChars / 2) ||
                    (soundMode == NotificationSoundMode.PlayAtEnd && i == totalChars - 1);

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
        if (notificationText == null)
            yield break;

        while (notificationText.text.Length > 0)
        {
            notificationText.text =
                notificationText.text.Substring(0, notificationText.text.Length - 1);

            yield return new WaitForSeconds(deleteSpeed);
        }

        StartCoroutine(FadeIndicator(false)); // fade-out indicator

        StartCoroutine(FadeOutNotification(0.2f));
        yield return new WaitForSeconds(0.2f);
    }

    private IEnumerator FadeOutNotification(float duration)
    {
        if (notificationText == null)
            yield break;

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

        notificationText.text = "";
    }

    private void SetNotificationAlpha(float alpha)
    {
        if (notificationText == null)
            return;

        Color c = notificationText.color;
        c.a = alpha;
        notificationText.color = c;
    }
}
