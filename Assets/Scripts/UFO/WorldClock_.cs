using UnityEngine;

public class WorldClock_ : MonoBehaviour
{
    [Header("Time Settings")]
    [Tooltip("Total real-time duration (in minutes) for a full in-game day-night cycle.")]
    public float totalCycleMinutes = 30f;

    [Tooltip("Duration (in minutes) of the day period within the cycle.")]
    public float dayDurationMinutes = 10f;

    [Tooltip("Duration (in minutes) of the night period within the cycle.")]
    public float nightDurationMinutes = 20f;

    [Header("Lighting Settings")]
    [Tooltip("Directional Light representing the Sun in your scene.")]
    public Light sunLight;

    [Tooltip("Sunlight intensity during the daytime.")]
    public float dayLightIntensity = 1f;

    [Tooltip("Sunlight intensity during the nighttime.")]
    public float nightLightIntensity = 0.1f;

    [Tooltip("Enable or disable Skybox color shifting based on time.")]
    public bool useSkybox = true;

    [Header("Skybox Settings")]
    [Tooltip("Exposure value when it is fully day.")]
    public float daySkyExposure = 1f;

    [Tooltip("Exposure value when it is fully night.")]
    public float nightSkyExposure = 0.3f;

    [Tooltip("Atmospheric thickness when it is day (thicker atmosphere).")]
    public float dayAtmosphereThickness = 2.5f;

    [Tooltip("Atmospheric thickness when it is night (thinner atmosphere).")]
    public float nightAtmosphereThickness = 0.12f;

    [Header("Runtime Info (Read-Only)")]
    [Tooltip("Current in-game time (0 to totalCycleMinutes).")]
    public float currentTime = 0f;

    [Tooltip("True if it is currently daytime.")]
    public bool isDay = true;

    // Cached reference to original skybox material (optional)
    private Material skyboxMaterial;

    private void Start()
    {
        currentTime = 0f;
        skyboxMaterial = RenderSettings.skybox;

        if (useSkybox && skyboxMaterial == null)
        {
            Debug.Log("You are not currently using the Skybox feature.");
            useSkybox = false;
        }

        if (sunLight == null)
        {
            Debug.Log("You are not currently using the Skybox feature.");
        }
    }

    private void Update()
    {
        // Advance time based on real-time progression
        currentTime += Time.deltaTime / 60f; // Convert seconds to minutes

        if (currentTime >= totalCycleMinutes)
            currentTime = 0f;

        // Determine whether it's day or night
        isDay = currentTime < dayDurationMinutes;

        UpdateLighting();
    }

    private void UpdateLighting()
    {
        if (sunLight != null)
        {
            float targetIntensity = isDay ? dayLightIntensity : nightLightIntensity;
            sunLight.intensity = Mathf.Lerp(sunLight.intensity, targetIntensity, Time.deltaTime * 2f);
        }

        if (useSkybox && skyboxMaterial != null)
        {
            // Exposure interpolation
            float targetExposure = isDay ? daySkyExposure : nightSkyExposure;
            if (skyboxMaterial.HasProperty("_Exposure"))
            {
                float currentExposure = skyboxMaterial.GetFloat("_Exposure");
                float newExposure = Mathf.Lerp(currentExposure, targetExposure, Time.deltaTime * 2f);
                skyboxMaterial.SetFloat("_Exposure", newExposure);
            }

            // Atmosphere thickness interpolation
            if (skyboxMaterial.HasProperty("_AtmosphereThickness"))
            {
                float targetThickness = isDay ? dayAtmosphereThickness : nightAtmosphereThickness;
                float currentThickness = skyboxMaterial.GetFloat("_AtmosphereThickness");
                float newThickness = Mathf.Lerp(currentThickness, targetThickness, Time.deltaTime * 2f);
                skyboxMaterial.SetFloat("_AtmosphereThickness", newThickness);
            }
        }
    }
}
