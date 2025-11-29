using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class LooseScene : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI loseText; // TMP text reference

    [Header("Text Settings")]
    [TextArea(3, 5)]
    public string loseMessage = "All the cows are gone...\nYour farm has fallen silent.";
    public float typeSpeed = 0.05f; // Speed per character
    public float textDisplayDuration = 3f; // How long the text stays before hiding

    [Header("Sound Settings")]
    public AudioSource audioSource;
    public AudioClip loseSound;

    [Header("Timing Settings")]
    public float initialDelay = 2f;  // Wait before text starts typing
    public float afterTextDelay = 2f; // Wait after text hides
    public float afterSoundDelay = 2f; // Wait after sound before loading credits

    [Header("Scene Settings")]
    public string creditsSceneName = "CreditsScene";

    private void Start()
    {
        if (loseText != null)
            loseText.text = "";

        StartCoroutine(LooseSequenceRoutine());
    }

    private IEnumerator LooseSequenceRoutine()
    {
        // Wait before typing
        yield return new WaitForSeconds(initialDelay);

        // Typewriting animation
        yield return StartCoroutine(TypeText(loseMessage));

        // Hold text for a few seconds
        yield return new WaitForSeconds(textDisplayDuration);

        // Hide text
        if (loseText != null)
            loseText.gameObject.SetActive(false);

        // Wait before sound
        yield return new WaitForSeconds(afterTextDelay);

        // Play lose sound
        if (audioSource != null && loseSound != null)
        {
            audioSource.PlayOneShot(loseSound);
        }

        // Wait before loading credits
        yield return new WaitForSeconds(afterSoundDelay);

        // Transition to credits scene
        SceneManager.LoadScene(creditsSceneName);
    }

    private IEnumerator TypeText(string message)
    {
        if (loseText == null)
            yield break;

        loseText.gameObject.SetActive(true);
        loseText.text = "";

        foreach (char c in message)
        {
            loseText.text += c;
            yield return new WaitForSeconds(typeSpeed);
        }
    }
}
