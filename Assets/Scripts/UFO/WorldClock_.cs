using System.Collections;
using UnityEngine;
using TMPro;

public class WorldClock_ : MonoBehaviour
{
    [Header("Time Settings")]
    [Tooltip("Total length of a full day-night cycle in real minutes.")]
    public float totalCycleMinutes = 30f;

    [Tooltip("Daytime duration in minutes.")]
    public float dayMinutes = 10f;

    [Tooltip("Nighttime duration in minutes.")]
    public float nightMinutes = 20f;

    [Tooltip("Optional Skybox Material with Procedural Shader.")]
    public Material skyboxMaterial;

    [Tooltip("Sun Light Reference (directional light).")]
    public Light sunLight;

    [Tooltip("Sun intensity at day and night.")]
    public float daySunIntensity = 1f;
    public float nightSunIntensity = 0.1f;

    [Tooltip("Atmospheric thickness day/night (skybox).")]
    public float dayAtmosphereThickness = 2.5f;
    public float nightAtmosphereThickness = 0.12f;

    [Header("Night Message Settings")]
    [Tooltip("TMP text that displays the 'Night X has started' message.")]
    public TMP_Text messageText;

    [Tooltip("Typing speed (seconds per character). Lower = faster.")]
    public float typingSpeed = 0.05f;

    [Tooltip("How long to keep the message visible after typing completes.")]
    public float messageDisplayDuration = 3f;

    [Tooltip("Enable or disable typewriter message feature.")]
    public bool enableNightMessages = true;

    // Runtime
    [HideInInspector] public bool isDay = true;
    private float currentTime;
    private float totalCycleSeconds;
    private bool transitioningToNight = false;
    private int nightCount = 0;

    void Start()
    {
        totalCycleSeconds = totalCycleMinutes * 60f;

        if (messageText != null)
        {
            messageText.gameObject.SetActive(false);
            messageText.text = "";
        }
    }

    void Update()
    {
        currentTime += Time.deltaTime;
        if (currentTime > totalCycleSeconds)
        {
            currentTime = 0f;
            transitioningToNight = false;
            isDay = true;
        }

        float daySeconds = dayMinutes * 60f;
        bool nowDay = currentTime < daySeconds;
        isDay = nowDay;

        // ----------------- Skybox + Light control -----------------
        float t = nowDay
            ? Mathf.InverseLerp(0f, daySeconds, currentTime)
            : Mathf.InverseLerp(daySeconds, totalCycleSeconds, currentTime);

        float sunIntensity = nowDay
            ? Mathf.Lerp(daySunIntensity, nightSunIntensity, t)
            : Mathf.Lerp(nightSunIntensity, daySunIntensity, t);

        if (sunLight)
            sunLight.intensity = sunIntensity;

        if (skyboxMaterial && skyboxMaterial.shader.name.Contains("Skybox/Procedural"))
        {
            float atmo = nowDay
                ? Mathf.Lerp(dayAtmosphereThickness, nightAtmosphereThickness, t)
                : Mathf.Lerp(nightAtmosphereThickness, dayAtmosphereThickness, t);

            skyboxMaterial.SetFloat("_AtmosphereThickness", atmo);
        }
        else if (!skyboxMaterial)
        {
            Debug.Log("You are not currently using the skybox feature.");
        }

        // ----------------- Detect transition into night -----------------
        if (!transitioningToNight && !nowDay)
        {
            transitioningToNight = true;
            nightCount++;
            if (enableNightMessages && messageText != null)
            {
                StartCoroutine(TypeNightMessage($"Night {nightCount} has started. Stay alert!"));
            }
        }

        // Reset flag once a full day has passed
        if (transitioningToNight && nowDay)
        {
            transitioningToNight = false;
        }
    }

    private IEnumerator TypeNightMessage(string msg)
    {
        messageText.gameObject.SetActive(true);
        messageText.text = "";

        foreach (char c in msg)
        {
            messageText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        yield return new WaitForSeconds(messageDisplayDuration);

        messageText.gameObject.SetActive(false);
        messageText.text = "";
    }
}
