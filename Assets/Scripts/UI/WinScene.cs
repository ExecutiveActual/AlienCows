using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class WinScene : MonoBehaviour
{
    [Header("UI References (Player)")]
    public TextMeshProUGUI dialogueText;        // 2D UI TMP for player dialogue
    public Button responseButton;               // Player response button
    public TextMeshProUGUI responseButtonText;  // Text of response button
    public Button continueButton;               // Final continue button

    [Header("3D Cow Dialogue")]
    public TextMeshPro cow3DText;               // World-space TMP for cow dialogue
    public Transform cowTextOrigin;             // Optional anchor for text (not mandatory)

    [Header("Dialogue Flow")]
    [TextArea(2, 4)] public string introText;   // Opening victory line
    [TextArea(2, 4)] public string[] cowTexts;  // Cow's dialogue sequence
    [TextArea(2, 4)] public string[] playerResponses; // Player's button responses

    [Header("Settings")]
    public float introDelay = 2f;
    public float typeSpeed = 0.03f;
    public float cowTypeSpeed = 0.05f;
    public string mainMenuSceneName = "MainMenu";

    private int conversationIndex = 0;
    private bool isTyping = false;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        // Initialize UI
        cow3DText.text = "";
        continueButton.gameObject.SetActive(false);
        responseButton.gameObject.SetActive(false);

        StartCoroutine(BeginSequence());
    }

    private IEnumerator BeginSequence()
    {
        yield return new WaitForSeconds(introDelay);
        yield return StartCoroutine(TypeText(dialogueText, introText, typeSpeed));

        // Show first player response
        responseButtonText.text = playerResponses[0];
        responseButton.gameObject.SetActive(true);
        responseButton.onClick.AddListener(OnPlayerRespond);
    }

    private void OnPlayerRespond()
    {
        if (isTyping) return;
        responseButton.gameObject.SetActive(false);
        StartCoroutine(CowRespondSequence());
    }

    private IEnumerator CowRespondSequence()
    {
        if (conversationIndex < cowTexts.Length)
        {
            // Cow speaks directly (no "..." now)
            cow3DText.text = "";
            yield return StartCoroutine(TypeText(cow3DText, cowTexts[conversationIndex], cowTypeSpeed));
            yield return new WaitForSeconds(0.4f);

            conversationIndex++;

            if (conversationIndex < playerResponses.Length)
            {
                responseButtonText.text = playerResponses[conversationIndex];
                responseButton.gameObject.SetActive(true);
            }
            else
            {
                // Conversation finished
                yield return new WaitForSeconds(1.2f);
                continueButton.gameObject.SetActive(true);
                continueButton.onClick.AddListener(OnContinuePressed);
            }
        }
    }

    private IEnumerator TypeText(TMP_Text target, string fullText, float speed)
    {
        isTyping = true;
        target.text = "";
        foreach (char c in fullText)
        {
            target.text += c;
            yield return new WaitForSecondsRealtime(speed);
        }
        isTyping = false;
    }

    private void OnContinuePressed()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
