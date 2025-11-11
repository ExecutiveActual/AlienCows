using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class NightEnd_FadeIn : MonoBehaviour
{
    
    [SerializeField] private float fadeInDuration = 2.0f; // Duration of the fade-in effect in seconds

    [SerializeField] private List<CanvasRenderer> canvasRenderers;


    Coroutine fadeInCoroutine;

    private void Awake()
    {
        canvasRenderers = new List<CanvasRenderer>(GetComponentsInChildren<CanvasRenderer>());
    }

    private void Start()
    {
        GetComponent<NightEnd_Manager>().UE_OnNightEndScreenFadeIn.AddListener(StartFadeIn);

        foreach (var canvasRenderer in canvasRenderers)
        {
            canvasRenderer.gameObject.SetActive(false);
        }
    }


    private void StartFadeIn()
    {
        Debug.Log("Starting Night End Fade-In");

        if (fadeInCoroutine != null)
        {
            StopCoroutine(fadeInCoroutine);
        }
        fadeInCoroutine = StartCoroutine(FadeInCoroutine());
    }


    private System.Collections.IEnumerator FadeInCoroutine()
    {
        float elapsedTime = 0f;
        // Set initial alpha to 0
        SetAlpha(0f);
        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsedTime / fadeInDuration);
            SetAlpha(alpha);
            yield return null;
        }
        // Ensure alpha is set to 1 at the end
        SetAlpha(1f);

        Debug.Log("Night End Fade-In Complete");
    }

    private void SetAlpha(float alpha)
    {
        foreach (var canvasRenderer in canvasRenderers)
        {
            canvasRenderer.SetAlpha(alpha);

            if (alpha == 0f)
            {
                canvasRenderer.gameObject.SetActive(true);
            }

        }
    }


}
