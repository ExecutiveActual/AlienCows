using System.Collections;
using UnityEngine;
using TMPro;

public class UFO_UI_Manager : MonoBehaviour
{
    [Header("Wave Info")]
    public TMP_Text waveText;

    [Header("In-Game UI")]
    public TMP_Text ufosDownText;
    public TMP_Text ufosFledText;
    public TMP_Text cowsLostText;
    public TMP_Text moneyText;

    [Header("Result Panel")]
    public GameObject resultPanel;
    public TMP_Text resultMoney;
    public TMP_Text resultDown;
    public TMP_Text resultFled;
    public TMP_Text resultCows;
    public float resultDelay = 1f;

    [Header("Toasts")]
    public GameObject toastPrefab;
    public Transform toastParent;

    void Awake()
    {
        if (resultPanel) resultPanel.SetActive(false);
        if (waveText) waveText.gameObject.SetActive(false);
    }


    // ------------------- WAVE UI -------------------
    public void ShowWaveStart(int night)
    {
        if (!waveText) return;

        waveText.text = "Night " + night.ToString();
        waveText.gameObject.SetActive(true);

        StartCoroutine(HideWave());
    }

    IEnumerator HideWave()
    {
        yield return new WaitForSeconds(3f);
        waveText.gameObject.SetActive(false);
    }


    // ------------------- LIVE UPDATES -------------------
    public void UpdateUFODownText(int count)
    {
        if (ufosDownText)
            ufosDownText.text = $"UFOs Down: x{count}";
    }

    public void UpdateUFOFledText(int count)
    {
        if (ufosFledText)
            ufosFledText.text = $"UFOs Fled: x{count}";
    }

    public void UpdateCowsLostText(int count)
    {
        if (cowsLostText)
            cowsLostText.text = $"Cows Lost: x{count}";
    }

    public void UpdateMoneyText(int amt)
    {
        if (moneyText)
            moneyText.text = "+" + amt.ToString() + "$";
    }


    // ------------------- RESULT PANEL -------------------
    public void ShowResultPanel(SpawnerResult r)
    {
        StartCoroutine(ResultRoutine(r));
    }

    IEnumerator ResultRoutine(SpawnerResult r)
    {
        yield return new WaitForSeconds(resultDelay);

        if (resultPanel)
        {
            resultPanel.SetActive(true);

            if (resultMoney) resultMoney.text = "+$" + r.moneyGained;
            if (resultDown) resultDown.text = "UFOs Down: " + r.ufosKilled;
            if (resultFled) resultFled.text = "UFOs Fled: " + r.ufosFled;
            if (resultCows) resultCows.text = "Cows Lost: " + r.cowsLost;
        }
    }


    // ------------------- TOAST -------------------
    public void ShowToast(string msg, float duration)
    {
        if (!toastPrefab || !toastParent) return;
        StartCoroutine(ToastRoutine(msg, duration));
    }

    IEnumerator ToastRoutine(string msg, float dur)
    {
        GameObject t = Instantiate(toastPrefab, toastParent);
        TMP_Text text = t.GetComponentInChildren<TMP_Text>();
        CanvasGroup g = t.GetComponent<CanvasGroup>();

        if (g) g.alpha = 0;

        // fade in
        float a = 0;
        while (a < 1)
        {
            a += Time.deltaTime * 4;
            if (g) g.alpha = a;
            yield return null;
        }

        // typewriter
        text.text = "";
        foreach (char c in msg)
        {
            text.text += c;
            yield return new WaitForSeconds(0.03f);
        }

        yield return new WaitForSeconds(dur);

        // typewriter reverse
        for (int i = text.text.Length - 1; i >= 0; i--)
        {
            text.text = text.text.Substring(0, i);
            yield return new WaitForSeconds(0.015f);
        }

        // fade out
        a = 1;
        while (a > 0)
        {
            a -= Time.deltaTime * 4;
            if (g) g.alpha = a;
            yield return null;
        }

        Destroy(t);
    }
}
