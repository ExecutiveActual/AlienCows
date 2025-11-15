using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Industrial-grade world clock that cycles through multiple nights and days.
/// Each night is preceded by a PreGame session that displays a custom notification
/// with typewriter-in and delete-out animations, plus an optional notification SFX.
/// </summary>
public class WorldClock_ : MonoBehaviour
{
    //===========================//
    //      ENUM DEFINITIONS     //
    //===========================//

    public enum TimeSession { PreGame, InGame_Night, InGame_Day, Idle }

    public enum NotificationSoundMode
    {
        PlayAtStart,   // Plays as soon as typing begins
        PlayAtMid,     // Plays halfway through typing
        PlayAtEnd      // Plays when the full message is typed
    }


    //===========================//
    //        TIME CONFIG        //
    //===========================//

    [Header("Time Durations (Seconds)")]
    [Tooltip("Time after delete animation before night gameplay starts.")]
    public float preGameDelayAfterDelete = 0.5f;

    [Tooltip("Duration of each night phase.")]
    public float nightDuration = 60f;

    [Tooltip("Duration of each day phase.")]
    public float dayDuration = 60f;

    [Tooltip("Total number of playable nights before the cycle ends.")]
    public int totalNights = 15;


    //===========================//
    //        UI SETTINGS        //
    //===========================//

    [Header("Night Notifications")]
    [Tooltip("Messages shown before each night. One entry per night.")]
    public List<string> nightNotificationMessages = new List<string>();

    [Tooltip("TMP text element for showing notifications.")]
    public TMP_Text notificationText;

    [Tooltip("Time between each letter appearing (in seconds).")]
    [Range(0.001f, 0.2f)]
    public float typeSpeed = 0.04f;

    [Tooltip("Time between each letter being deleted (in seconds).")]
    [Range(0.001f, 0.2f)]
    public float deleteSpeed = 0.02f;

    [Tooltip("Time message stays on-screen before deletion begins.")]
    public float messageHoldTime = 1.5f;


    //===========================//
    //     AUDIO (NOTIFICATION)  //
    //===========================//

    [Header("Notification Audio")]
    [Tooltip("AudioSource used for notification playback.")]
    public AudioSource notificationAudioSource;

    [Tooltip("Sound clip played once per message.")]
    public AudioClip notificationClip;

    [Tooltip("Notification playback volume.")]
    [Range(0f, 1f)]
    public float notificationVolume = 1f;

    [Tooltip("When the notification sound plays relative to typing animation.")]
    public NotificationSoundMode soundMode = NotificationSoundMode.PlayAtStart;


    //===========================//
    //     SKY & LIGHT SETTINGS  //
    //===========================//

    [Header("Skybox & Lighting")]
    [Tooltip("Skybox material for environment transitions.")]
    public Material skyboxMaterial;

    [Tooltip("Directional light representing the sun.")]
    public Light sunLight;

    [Tooltip("Atmosphere thickness at night (lower = thinner).")]
    [Range(0f, 1f)]
    public float nightAtmosphereThickness = 0.15f;

    [Tooltip("Atmosphere thickness at day (higher = denser).")]
    [Range(0f, 1f)]
    public float dayAtmosphereThickness = 1f;

    [Tooltip("Sunlight intensity at night.")]
    public float nightSunIntensity = 0.1f;

    [Tooltip("Sunlight intensity at day.")]
    public float daySunIntensity = 1.2f;

    [Tooltip("Speed of sky/lighting blend transitions.")]
    [Range(0.05f, 2f)]
    public float transitionSpeed = 0.3f;


    //===========================//
    //        STATE DATA         //
    //===========================//

    [Tooltip("Current time session for external reference.")]
    public TimeSession currentSession { get; private set; }

    [Tooltip("Current active night number.")]
    public int CurrentNight { get; private set; } = 0;

    [Tooltip("True when all nights are complete.")]
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
        if (nightNotificationMessages != null && nightNotificationMessages.Count >= nightIndex && !string.IsNullOrEmpty(nightNotificationMessages[nightIndex - 1]))
            message = nightNotificationMessages[nightIndex - 1];

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
    //       TYPEWRITER UI       //
    //===========================//

    private IEnumerator TypewriterIn(string message)
    {
        SetNotificationAlpha(1f);
        notificationText.text = "";

        int totalChars = message.Length;
        bool soundPlayed = false;

        for (int i = 0; i < totalChars; i++)
        {
            notificationText.text += message[i];

            // --- SOUND TIMING CONTROL ---
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
            // ----------------------------

            yield return new WaitForSeconds(typeSpeed);
        }
    }

    private IEnumerator TypewriterOut()
    {
        if (notificationText == null)
            yield break;

        while (notificationText.text.Length > 0)
        {
            notificationText.text = notificationText.text.Substring(0, notificationText.text.Length - 1);
            yield return new WaitForSeconds(deleteSpeed);
        }

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

