using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NotificationSequence : MonoBehaviour
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

    [Header("Text Content")]
    [TextArea] 
    public string notificationText = "Mission Started";

    [Header("Visual Settings")]
    [Range(0f, 1f)]
    public float backgroundTargetAlpha = 0.35f; // BG max alpha when fully visible

    private void Start()
    {
        // Initialize visuals
        if (backgroundImage) SetAlpha(backgroundImage, 0);
        if (iconImage) SetAlpha(iconImage, 0);
        if (uiText) uiText.text = "";

        StartCoroutine(PlayNotification());
    }

    private IEnumerator PlayNotification()
    {
        // Delay start
        yield return new WaitForSeconds(startDelay);

        // Fade in background (to 35%)
        yield return StartCoroutine(FadeTo(backgroundImage, fadeDuration, backgroundTargetAlpha));

        // Fade in icon slightly after
        yield return new WaitForSeconds(iconDelay);
        yield return StartCoroutine(FadeTo(iconImage, fadeDuration, 1f));

        // Wait before text
        yield return new WaitForSeconds(textDelay);

        // Play sound
        if (audioSource && startSound)
            audioSource.PlayOneShot(startSound);

        // Type in text
        yield return StartCoroutine(TypeText());

        // Wait visible
        yield return new WaitForSeconds(stayDuration);

        // Delete text
        yield return StartCoroutine(DeleteText());

        // Fade out icon
        yield return StartCoroutine(FadeTo(iconImage, fadeDuration, 0f));

        // Fade out background last (to 0)
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
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, t / duration);
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
