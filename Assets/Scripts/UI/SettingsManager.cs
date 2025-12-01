using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SettingsManager : MonoBehaviour
{
    [Header("Audio")]
    public Slider masterVolumeSlider;

    public SO_SoundVolumeSetting volumeSetting;

    [Header("Graphics")]
    public Dropdown resolutionDropdown;
    public Dropdown qualityDropdown;
    public Toggle fullscreenToggle;

    [Header("UI Scaling")]
    public CanvasScaler canvasScaler; // Assign your main CanvasScaler here

    private Resolution[] customResolutions;

    private void Awake()
    {
        // Clear listeners in case this object is re-enabled
        if (masterVolumeSlider != null) masterVolumeSlider.onValueChanged.RemoveAllListeners();
        if (resolutionDropdown != null) resolutionDropdown.onValueChanged.RemoveAllListeners();
        if (qualityDropdown != null) qualityDropdown.onValueChanged.RemoveAllListeners();
        if (fullscreenToggle != null) fullscreenToggle.onValueChanged.RemoveAllListeners();
    }

    private void Start()
    {
        // =====================================
        // Define 3 Custom Resolution Options
        // =====================================
        customResolutions = new Resolution[3];
        customResolutions[0] = new Resolution { width = 2560, height = 1440 }; // default
        customResolutions[1] = new Resolution { width = 1920, height = 1080 };
        customResolutions[2] = new Resolution { width = 1280, height = 720 };

        // =====================================
        // Populate Resolution Dropdown
        // =====================================
        if (resolutionDropdown != null)
        {
            resolutionDropdown.ClearOptions();
            List<string> options = new List<string>();
            foreach (var res in customResolutions)
                options.Add($"{res.width} x {res.height}");
            resolutionDropdown.AddOptions(options);
        }

        // =====================================
        // Populate Quality Dropdown
        // =====================================
        if (qualityDropdown != null)
        {
            qualityDropdown.ClearOptions();
            qualityDropdown.AddOptions(new List<string>(QualitySettings.names));
        }

        // =====================================
        // Load Saved Settings
        // =====================================
        int savedRes = PlayerPrefs.GetInt("ResolutionIndex", 0); // default 2560x1440
        int savedQuality = PlayerPrefs.GetInt("QualityIndex", QualitySettings.GetQualityLevel());
        bool savedFullscreen = PlayerPrefs.GetInt("Fullscreen", Screen.fullScreen ? 1 : 0) == 1;

        float savedMaster = PlayerPrefs.GetFloat("MasterVol", 1f);

        // Clamp indices for safety (in case prefs are from an older build)
        if (savedRes < 0 || savedRes >= customResolutions.Length) savedRes = 0;
        if (savedQuality < 0 || savedQuality >= QualitySettings.names.Length)
            savedQuality = QualitySettings.GetQualityLevel();

        // =====================================
        // Apply Loaded Settings
        // =====================================
        ApplyResolution(savedRes);
        ApplyQuality(savedQuality);
        ApplyFullscreen(savedFullscreen);
        UpdateCanvasScaler(); // ensure UI scaling matches current resolution

        // =====================================
        // Initialize UI with Saved Values
        // =====================================
        if (resolutionDropdown != null) resolutionDropdown.value = savedRes;
        if (qualityDropdown != null) qualityDropdown.value = savedQuality;
        if (fullscreenToggle != null) fullscreenToggle.isOn = savedFullscreen;

        if (masterVolumeSlider != null) masterVolumeSlider.value = savedMaster;

        // =====================================
        // Force Audio Sync Once (fix mute-on-open)
        // =====================================
        OnMasterVolumeChange(savedMaster);

        // =====================================
        // Add Listeners
        // =====================================
        if (masterVolumeSlider != null)
            masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChange);

        if (resolutionDropdown != null)
            resolutionDropdown.onValueChanged.AddListener(OnResolutionChange);

        if (qualityDropdown != null)
            qualityDropdown.onValueChanged.AddListener(OnQualityChange);

        if (fullscreenToggle != null)
            fullscreenToggle.onValueChanged.AddListener(OnFullscreenToggle);
    }

    // =============================
    // AUDIO CONTROLS
    // =============================
    public void OnMasterVolumeChange(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetMasterVolume(value);



        PlayerPrefs.SetFloat("MasterVol", value);
    }


    // =============================
    // GRAPHICS CONTROLS
    // =============================
    public void OnResolutionChange(int index)
    {
        ApplyResolution(index);
        UpdateCanvasScaler(); // auto-adjust canvas when res changes

        PlayerPrefs.SetInt("ResolutionIndex", index);
        Debug.Log($"[SettingsManager] Resolution set to {customResolutions[index].width}x{customResolutions[index].height}");
    }

    public void OnQualityChange(int index)
    {
        ApplyQuality(index);
        PlayerPrefs.SetInt("QualityIndex", index);
        Debug.Log($"[SettingsManager] Quality set to {QualitySettings.names[index]}");
    }

    public void OnFullscreenToggle(bool isFullscreen)
    {
        ApplyFullscreen(isFullscreen);
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
        Debug.Log($"[SettingsManager] Fullscreen: {isFullscreen}");
    }

    // =============================
    // APPLY METHODS
    // =============================
    private void ApplyResolution(int index)
    {
        if (index < 0 || index >= customResolutions.Length) return;

        Resolution res = customResolutions[index];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen);
    }

    private void ApplyQuality(int index)
    {
        if (index < 0 || index >= QualitySettings.names.Length) return;

        QualitySettings.SetQualityLevel(index, true);
    }

    private void ApplyFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    // =============================
    // CANVAS SCALING
    // =============================
    private void UpdateCanvasScaler()
    {
        if (canvasScaler == null) return;

        // Make sure UI scales nicely across your 3 resolutions
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(2560f, 1440f); // your "design" resolution
        canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        canvasScaler.matchWidthOrHeight = 0.5f; // 0 = width, 1 = height, 0.5 = balanced
    }

    private void OnDestroy()
    {
        PlayerPrefs.Save();
    }
}
