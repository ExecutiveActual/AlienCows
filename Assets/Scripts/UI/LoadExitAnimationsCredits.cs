using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class LoadExitAnimationCredits : MonoBehaviour
{
    public Animator firstAnimator;
    public Animator secondAnimator;
    public float animationWaitTime = 2f;

    public TMP_Text textToWrite;
    [TextArea(3, 5)] public string sentence;
    public float waitBeforeTyping = 1f;
    public float typeSpeed = 0.05f;
    public float waitAfterTyping = 1.5f;

    public Image fadeImage;
    public float fadeSpeed = 1f;
    public string mainMenuSceneName = "MainMenu";

    private bool isExiting = false;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(false);
            fadeImage.color = new Color(0, 0, 0, 0);
        }

        if (textToWrite != null)
        {
            textToWrite.gameObject.SetActive(false);
            textToWrite.text = "";
        }
    }

    public void OnBackToMainMenuPressed()
    {
        if (isExiting) return;
        isExiting = true;
        StartCoroutine(PlayExitSequence());
    }

    private IEnumerator PlayExitSequence()
    {
        if (firstAnimator != null)
            firstAnimator.SetTrigger("1_TransitionDown");

        if (secondAnimator != null)
            secondAnimator.SetTrigger("2_TransitionUp");

        yield return new WaitForSeconds(animationWaitTime);
        yield return StartCoroutine(TypewriterSequence());
    }

    private IEnumerator TypewriterSequence()
    {
        if (textToWrite == null)
            yield break;

        textToWrite.text = "";

        if (waitBeforeTyping > 0f)
        {
            if (waitBeforeTyping > 1f)
                yield return new WaitForSeconds(waitBeforeTyping - 1f);

            textToWrite.gameObject.SetActive(true);
            yield return new WaitForSeconds(Mathf.Min(1f, waitBeforeTyping));
        }
        else
        {
            textToWrite.gameObject.SetActive(true);
        }

        foreach (char c in sentence)
        {
            textToWrite.text += c;
            yield return new WaitForSeconds(typeSpeed);
        }

        yield return new WaitForSeconds(waitAfterTyping);

        if (fadeImage != null)
            fadeImage.gameObject.SetActive(true);

        yield return StartCoroutine(FadeOutAndLoad());
    }

    private IEnumerator FadeOutAndLoad()
    {
        if (fadeImage == null)
        {
            SceneManager.LoadScene(mainMenuSceneName);
            yield break;
        }

        Color col = fadeImage.color;
        while (col.a < 1f)
        {
            col.a = Mathf.Min(1f, col.a + Time.deltaTime * fadeSpeed);
            fadeImage.color = col;
            yield return null;
        }

        SceneManager.LoadScene(mainMenuSceneName);
    }
}
