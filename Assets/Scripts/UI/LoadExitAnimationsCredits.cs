using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class LoadExitAnimationCredits : MonoBehaviour
{
    [Header("Animator References")]
    public Animator firstAnimator;   // Animator with 1_TransitionDown
    public Animator secondAnimator;  // Animator with 2_TransitionUp
    public float animationWaitTime = 2f; // Time to wait for animations to finish

    [Header("Typewriter Settings")]
    public TMP_Text textToWrite;             // The TMP text to display
    [TextArea(3, 5)] public string sentence; // Sentence to type out
    public float waitBeforeTyping = 1f;      // Wait before typing starts
    public float typeSpeed = 0.05f;          // Delay between each character
    public float waitAfterTyping = 1.5f;     // Wait after text finishes typing

    [Header("Fade Out Settings")]
    public Image fadeImage;                  // Fullscreen black image for fade out
    public float fadeSpeed = 1f;             // Speed of fade out
    public string mainMenuSceneName = "MainMenu"; // Scene name to load after fade

    [Header("UI Sound Settings")]
    public AudioSource uiAudioSource;        // Assign any audio source in scene
    public AudioClip clickSound;             // Click sound for button press
    public AudioClip hoverSound;             // Hover sound for OnPointerEnter

    private bool isExiting = false;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        // Ensure fade image starts disabled and transparent
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(false);
            fadeImage.color = new Color(0, 0, 0, 0);
        }

        // Ensure the text is disabled at start
        if (textToWrite != null)
        {
            textToWrite.gameObject.SetActive(false);
            textToWrite.text = "";
        }

        // Auto-add AudioSource if missing
        if (uiAudioSource == null)
        {
            uiAudioSource = gameObject.AddComponent<AudioSource>();
            uiAudioSource.playOnAwake = false;
        }
    }

    /// <summary>
    /// Call this from your "Back to Main Menu" button's OnClick event
    /// </summary>
    public void OnBackToMainMenuPressed()
    {
        if (isExiting) return;
        isExiting = true;

        // Play click sound immediately
        if (clickSound != null && uiAudioSource != null)
            uiAudioSource.PlayOneShot(clickSound);

        // Start cinematic exit sequence
        StartCoroutine(PlayExitSequence());
    }

    /// <summary>
    /// Call this from the button's OnPointerEnter event (via EventTrigger)
    /// </summary>
    public void PlayHoverSound()
    {
        if (hoverSound != null && uiAudioSource != null)
            uiAudioSource.PlayOneShot(hoverSound);
    }

    private IEnumerator PlayExitSequence()
    {
        // Trigger both animations simultaneously
        if (firstAnimator != null)
            firstAnimator.SetTrigger("1_TransitionDown");

        if (secondAnimator != null)
            secondAnimator.SetTrigger("2_TransitionUp");

        // Wait for the animations to finish
        yield return new WaitForSeconds(animationWaitTime);

        // Begin the typewriter sequence
        yield return StartCoroutine(TypewriterSequence());
    }

    private IEnumerator TypewriterSequence()
    {
        if (textToWrite == null)
        {
            Debug.LogWarning("No TMP_Text assigned for typing sequence!");
            yield break;
        }

        textToWrite.text = "";

        // Wait before typing starts
        if (waitBeforeTyping > 0f)
        {
            // Enable text a second before typing begins (if waitBeforeTyping > 1)
            if (waitBeforeTyping > 1f)
                yield return new WaitForSeconds(waitBeforeTyping - 1f);

            textToWrite.gameObject.SetActive(true);

            // Remaining wait time until typing begins
            yield return new WaitForSeconds(Mathf.Min(1f, waitBeforeTyping));
        }
        else
        {
            textToWrite.gameObject.SetActive(true);
        }

        // Typewriter effect
        foreach (char c in sentence.ToCharArray())
        {
            textToWrite.text += c;
            yield return new WaitForSeconds(typeSpeed);
        }

        // Wait a bit before fade-out
        yield return new WaitForSeconds(waitAfterTyping);

        // Activate fade image (still transparent)
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
        }

        // Start fade-out and scene load
        yield return StartCoroutine(FadeOutAndLoad());
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
