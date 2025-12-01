using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class LooseScene : MonoBehaviour
{
    public TextMeshProUGUI loseText;
    [TextArea(3, 5)]
    public string loseMessage = "All the cows are gone...\nYour farm has fallen silent.";
    public float typeSpeed = 0.05f;
    public float textDisplayDuration = 3f;
    public float initialDelay = 2f;
    public float afterTextDelay = 2f;
    public float afterSceneDelay = 2f;
    public string creditsSceneName = "CreditsScene";

    private void Start()
    {
        if (loseText != null)
            loseText.text = "";
        StartCoroutine(LooseSequenceRoutine());
    }

    private IEnumerator LooseSequenceRoutine()
    {
        yield return new WaitForSeconds(initialDelay);
        yield return StartCoroutine(TypeText(loseMessage));
        yield return new WaitForSeconds(textDisplayDuration);

        if (loseText != null)
            loseText.gameObject.SetActive(false);

        yield return new WaitForSeconds(afterTextDelay + afterSceneDelay);
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
