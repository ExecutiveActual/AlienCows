using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Notification : MonoBehaviour
{
    public Image backgroundImage;
    public Image iconImage;
    public TMP_Text uiText;

    public float startDelay = 1f;
    public float fadeDuration = 0.4f;
    public float iconDelay = 0.2f;
    public float textDelay = 0.2f;
    public float typeSpeed = 0.05f;
    public float stayDuration = 2f;
    public float deleteSpeed = 0.04f;

    [Range(0f, 1f)]
    public float backgroundTargetAlpha = 0.35f;

    [TextArea] 
    public string[] nightMessages = new string[]
    {
        "Night 1: The Theatre Awakens...",
        "Night 2: Shadows grow longer.",
        "Night 3: The streets remember you.",
        "Night 4: Whispers in the cinema hall.",
        "Night 5: Curtains rise for the finale."
    };

    private string notificationText = "Mission Started";
    private GameManager_NightCounter nightCounter;
    private Coroutine notificationCoroutine;

    private void Start()
    {
        if (backgroundImage) SetAlpha(backgroundImage, 0);
        if (iconImage) SetAlpha(iconImage, 0);
        if (uiText) uiText.text = "";

        if (GameManager_Singleton.Instance)
            nightCounter = GameManager_Singleton.Instance.GetComponent<GameManager_NightCounter>();

        UpdateNotificationTextFromNight();
        notificationCoroutine = StartCoroutine(PlayNotification());
    }

    private void UpdateNotificationTextFromNight()
    {
        if (nightCounter == null)
        {
            notificationText = "Mission Started";
            return;
        }

        int currentNight = nightCounter.GetNightNumberCurrent();

        if (currentNight - 1 >= 0 && currentNight - 1 < nightMessages.Length)
            notificationText = nightMessages[currentNight - 1];
        else
            notificationText = $"Night {currentNight}: Mission Continues...";
    }

    private IEnumerator PlayNotification()
    {
        yield return new WaitForSeconds(startDelay);
        yield return StartCoroutine(FadeTo(backgroundImage, fadeDuration, backgroundTargetAlpha));
        yield return new WaitForSeconds(iconDelay);
        yield return StartCoroutine(FadeTo(iconImage, fadeDuration, 1f));
        yield return new WaitForSeconds(textDelay);
        yield return StartCoroutine(TypeText());
        yield return new WaitForSeconds(stayDuration);
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
