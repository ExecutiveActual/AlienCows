using TMPro;
using UnityEngine;

/// <summary>
/// Tracks the number of cows abducted and updates the UI.
/// </summary>
public class AbductionTracker_ : MonoBehaviour
{
    public static AbductionTracker_ Instance;

    [Header("UI References")]
    [Tooltip("Text element displaying total abductions (TMP).")]
    public TextMeshProUGUI abductionCountText;

    [Tooltip("Optional abduction icon (will be hidden by default).")]
    public UnityEngine.UI.Image abductionIcon;

    [Header("Settings")]
    [Tooltip("Prefix shown before the count.")]
    public string prefix = "x ";

    [Tooltip("Should the text fade in on first abduction?")]
    public bool fadeInOnFirstAbduction = true;

    public float fadeSpeed = 2f;

    [HideInInspector] public int CurrentAbductions = 0;

    private bool firstShown = false;

    //──────────────────────────────────────────────

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Disabled by default
        if (abductionCountText)
            abductionCountText.gameObject.SetActive(false);

        if (abductionIcon)
            abductionIcon.gameObject.SetActive(false);
    }

    //──────────────────────────────────────────────

    public void RegisterAbduction()
    {
        CurrentAbductions++;

        // Enable UI on first abduction
        if (!firstShown)
        {
            firstShown = true;

            if (abductionIcon)
                abductionIcon.gameObject.SetActive(true);

            if (abductionCountText)
            {
                abductionCountText.gameObject.SetActive(true);
                if (fadeInOnFirstAbduction)
                    StartCoroutine(FadeInTMP(abductionCountText));
            }
        }

        UpdateText();
    }

    //──────────────────────────────────────────────

    private void UpdateText()
    {
        if (abductionCountText)
            abductionCountText.text = prefix + CurrentAbductions.ToString();
    }

    //──────────────────────────────────────────────
    // Optional fade-in for a cinematic reveal
    //──────────────────────────────────────────────
    private System.Collections.IEnumerator FadeInTMP(TextMeshProUGUI tmp)
    {
        tmp.alpha = 0f;
        while (tmp.alpha < 1f)
        {
            tmp.alpha += Time.deltaTime * fadeSpeed;
            yield return null;
        }
        tmp.alpha = 1f;
    }
}
