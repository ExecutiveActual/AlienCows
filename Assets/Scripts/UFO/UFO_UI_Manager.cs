using System.Collections;
using UnityEngine;
using UnityEngine.UI;
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

    [Header("Blink Icons")]
    public Image iconUFODown;
    public Image iconUFOFled;
    public Image iconCowLost;

    public float blinkDuration = 0.4f;  
    public int blinkCount = 2;

    // -------- Internal Counters (for accurate results panel) --------
    int rt_UFODown = 0;
    int rt_UFOFled = 0;
    int rt_CowsLost = 0;
    int rt_Money = 0;

    void Awake()
    {
        if (resultPanel) resultPanel.SetActive(false);
        if (waveText) waveText.gameObject.SetActive(false);

        InitIcon(iconUFODown);
        InitIcon(iconUFOFled);
        InitIcon(iconCowLost);
    }

    void InitIcon(Image i)
    {
        if (i)
        {
            Color c = i.color;
            c.a = 0;
            i.color = c;
        }
    }

    // ------------------- WAVE -------------------
    public void ShowWaveStart(int night)
    {
        if (!waveText) return;

        waveText.text = "Night " + night;
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
        rt_UFODown = count;

        if (ufosDownText)
            ufosDownText.text = $"UFOs Down: x{count}";

        Blink(iconUFODown);
    }

    public void UpdateUFOFledText(int count)
    {
        rt_UFOFled = count;

        if (ufosFledText)
            ufosFledText.text = $"UFOs Fled: x{count}";

        Blink(iconUFOFled);
    }

    public void UpdateCowsLostText(int count)
    {
        rt_CowsLost = count;

        if (cowsLostText)
            cowsLostText.text = $"Cows Lost: x{count}";

        Blink(iconCowLost);
    }

    public void UpdateMoneyText(int amt)
    {
        rt_Money = amt;

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

            // use REAL-TIME values instead of r values
            if (resultMoney) resultMoney.text = "+$" + rt_Money;
            if (resultDown) resultDown.text = "UFOs Down: " + rt_UFODown;
            if (resultFled) resultFled.text = "UFOs Fled: " + rt_UFOFled;
            if (resultCows) resultCows.text = "Cows Lost: " + rt_CowsLost;
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

        float a = 0;
        while (a < 1)
        {
            a += Time.deltaTime * 4;
            if (g) g.alpha = a;
            yield return null;
        }

        text.text = "";
        foreach (char c in msg)
        {
            text.text += c;
            yield return new WaitForSeconds(0.03f);
        }

        yield return new WaitForSeconds(dur);

        for (int i = text.text.Length - 1; i >= 0; i--)
        {
            text.text = text.text.Substring(0, i);
            yield return new WaitForSeconds(0.015f);
        }

        a = 1;
        while (a > 0)
        {
            a -= Time.deltaTime * 4;
            if (g) g.alpha = a;
            yield return null;
        }

        Destroy(t);
    }

    // ------------------- ICON BLINK -------------------
    void Blink(Image icon)
    {
        if (icon)
            StartCoroutine(BlinkRoutine(icon));
    }

    IEnumerator BlinkRoutine(Image icon)
    {
        for (int b = 0; b < blinkCount; b++)
        {
            float t = 0;
            while (t < blinkDuration)
            {
                t += Time.deltaTime;
                SetAlpha(icon, t / blinkDuration);
                yield return null;
            }

            t = 0;
            while (t < blinkDuration)
            {
                t += Time.deltaTime;
                SetAlpha(icon, 1 - (t / blinkDuration));
                yield return null;
            }
        }

        SetAlpha(icon, 0);
    }

    void SetAlpha(Image img, float a)
    {
        Color c = img.color;
        c.a = a;
        img.color = c;
    }
}
