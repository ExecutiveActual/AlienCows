using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class WaitAndType : MonoBehaviour
{
    [Header("Typewriter Settings")]
    public TMP_Text textToWrite;           // The TMP text to display
    [TextArea(3, 5)] public string sentence; // Sentence to type out
    public float waitBeforeTyping = 2f;    // Wait before typing starts
    public float typeSpeed = 0.05f;        // Delay between each character
    public float waitAfterTyping = 2f;     // Wait after text finishes typing

    [Header("Fade Out Settings")]
    public Image fadeImage;                // Fullscreen black image for fade out
    public float fadeSpeed = 1f;           // Speed of fade out
    public string mainMenuSceneName = "MainMenu"; // Scene name to load after fade

    private void Start()
    {
        if (textToWrite != null)
            textToWrite.text = "";

        if (fadeImage != null)
            fadeImage.color = new Color(0, 0, 0, 0); // start transparent

        StartCoroutine(WaitAndStartTyping());
    }

    private IEnumerator WaitAndStartTyping()
    {
        yield return new WaitForSeconds(waitBeforeTyping);
        yield return StartCoroutine(TypeSentence());
        yield return new WaitForSeconds(waitAfterTyping);
        yield return StartCoroutine(FadeOutAndLoad());
    }

    private IEnumerator TypeSentence()
    {
        textToWrite.text = "";

        foreach (char c in sentence.ToCharArray())
        {
            textToWrite.text += c;
            yield return new WaitForSeconds(typeSpeed);
        }
    }

    private IEnumerator FadeOutAndLoad()
    {
        if (fadeImage == null)
        {
            Debug.LogWarning("Fade image not assigned. Skipping fade-out.");
            SceneManager.LoadScene(mainMenuSceneName);
            yield break;
        }

        Color col = fadeImage.color;
        while (col.a < 1)
        {
            col.a += Time.deltaTime * fadeSpeed;
            fadeImage.color = col;
            yield return null;
        }

        SceneManager.LoadScene(mainMenuSceneName);
    }
}
