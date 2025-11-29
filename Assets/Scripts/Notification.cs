using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Notification : MonoBehaviour
{
    [Header("UI References")]
    public Image backgroundImage;      // Background (panel)
    public Image iconImage;            // Notification icon
    public TMP_Text uiText;            // TextMeshPro text

    [Header("Timing Settings")]
    public float startDelay = 1f;      // Wait before sequence starts
    public float fadeDuration = 0.4f;  // Duration for fade in/out
    public float iconDelay = 0.2f;     // Delay after bg before icon fades in
    public float textDelay = 0.2f;     // Delay after icon before typing
    public float typeSpeed = 0.05f;    // Time between letters appearing
    public float stayDuration = 2f;    // Time text stays visible
    public float deleteSpeed = 0.04f;  // Time between letters disappearing

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip startSound;

    [Header("Visual Settings")]
    [Range(0f, 1f)]
    public float backgroundTargetAlpha = 0.35f; // BG max alpha when fully visible

    [Header("Night Notification Texts")]
    [TextArea] public string[] nightMessages = new string[]
    {
        "Night 1: The Theatre Awakens...",
        "Night 2: Shadows grow longer.",
        "Night 3: The streets remember you.",
        "Night 4: Whispers in the cinema hall.",
        "Night 5: Curtains rise for the finale."
    };

    private string notificationText = "Mission Started";

    // Reference cache
    private GameManager_NightCounter nightCounter;
    private Coroutine notificationCoroutine;

    private void Start()
    {
        // Initialize visuals
        if (backgroundImage) SetAlpha(backgroundImage, 0);
        if (iconImage) SetAlpha(iconImage, 0);
        if (uiText) uiText.text = "";

        // Try to connect to the GameManager and NightCounter
        if (GameManager_Singleton.Instance)
            nightCounter = GameManager_Singleton.Instance.GetComponent<GameManager_NightCounter>();

        // Set text according to current night
        UpdateNotificationTextFromNight();

        // Begin the cinematic sequence
        notificationCoroutine = StartCoroutine(PlayNotification());
    }


    private void UpdateNotificationTextFromNight()
    {
        if (nightCounter == null)
        {
            notificationText = "Mission Started"; // fallback
            return;
        }

        int currentNight = nightCounter.GetNightNumberCurrent();

        // Choose text based on current night (with safety)
        if (currentNight - 1 >= 0 && currentNight - 1 < nightMessages.Length)
            notificationText = nightMessages[currentNight - 1];
        else
            notificationText = $"Night {currentNight}: Mission Continues...";
    }
    private IEnumerator PlayNotification()
    {
        yield return new WaitForSeconds(startDelay);

        // Fade in background
        yield return StartCoroutine(FadeTo(backgroundImage, fadeDuration, backgroundTargetAlpha));

        // Fade in icon
        yield return new WaitForSeconds(iconDelay);
        yield return StartCoroutine(FadeTo(iconImage, fadeDuration, 1f));

        // Wait before text typing
        yield return new WaitForSeconds(textDelay);

        // Play notification sound
        if (audioSource && startSound)
            audioSource.PlayOneShot(startSound);

        // Type in the text
        yield return StartCoroutine(TypeText());

        // Keep text visible for a while
        yield return new WaitForSeconds(stayDuration);

        // Delete text with fade-out sequence
        yield return StartCoroutine(DeleteText());
        yield return StartCoroutine(FadeTo(iconImage, fadeDuration, 0f));
        yield return StartCoroutine(FadeTo(backgroundImage, fadeDuration, 0f));

        uiText.text = "";
    }

    
    private IEnumerator TypeText()
    {
        uiText.text = "";
        foreach (char c in notificationText)
        {
            uiText.text += c;
            yield return new WaitForSeconds(typeSpeed);
        }
    }

    
    private IEnumerator DeleteText()
    {
        for (int i = notificationText.Length; i > 0; i--)
        {
            uiText.text = notificationText.Substring(0, i - 1);
            yield return new WaitForSeconds(deleteSpeed);
        }
    }

   
    private IEnumerator FadeTo(Graphic graphic, float duration, float targetAlpha)
    {
        if (graphic == null) yield break;

        float startAlpha = graphic.color.a;
        float t = 0f;
        Color c = graphic.color;

        while (t < duration)
        {
            t += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, Mathf.SmoothStep(0, 1, t / duration));
            c.a = newAlpha;
            graphic.color = c;
            yield return null;
        }

        c.a = targetAlpha;
        graphic.color = c;
    }

 
    private void SetAlpha(Graphic graphic, float alpha)
    {
        if (graphic == null) return;
        Color c = graphic.color;
        c.a = alpha;
        graphic.color = c;
    }
}
