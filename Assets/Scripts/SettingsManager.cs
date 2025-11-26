using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    [Header("Audio")]
    public Slider masterVolumeSlider;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;

    [Header("Graphics")]
    public Dropdown resolutionDropdown;
    public Dropdown qualityDropdown;
    public Toggle fullscreenToggle;

    private Resolution[] resolutions;

    private void Start()
    {
        // =========================
        // Populate Resolution List
        // =========================
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        var options = new System.Collections.Generic.List<string>();
        int currentResIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = $"{resolutions[i].width} x {resolutions[i].height}";
            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentResIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);

        // =========================
        // Load Saved or Defaults
        // =========================
        int savedRes = PlayerPrefs.GetInt("ResolutionIndex", currentResIndex);
        int savedQuality = PlayerPrefs.GetInt("QualityIndex", QualitySettings.GetQualityLevel());
        bool savedFullscreen = PlayerPrefs.GetInt("Fullscreen", Screen.fullScreen ? 1 : 0) == 1;

        float savedMaster = PlayerPrefs.GetFloat("MasterVol", 1f);
        float savedMusic = PlayerPrefs.GetFloat("MusicVol", 0.8f);
        float savedSFX = PlayerPrefs.GetFloat("SFXVol", 0.8f);

        // Apply immediately
        ApplyResolution(savedRes);
        ApplyQuality(savedQuality);
        ApplyFullscreen(savedFullscreen);

        // UI initialization
        resolutionDropdown.value = savedRes;
        qualityDropdown.ClearOptions();
        qualityDropdown.AddOptions(new System.Collections.Generic.List<string>(QualitySettings.names));
        qualityDropdown.value = savedQuality;
        fullscreenToggle.isOn = savedFullscreen;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMasterVolume(savedMaster);
            AudioManager.Instance.SetMusicVolume(savedMusic);
            AudioManager.Instance.SetSFXVolume(savedSFX);
        }

        masterVolumeSlider.value = savedMaster;
        musicVolumeSlider.value = savedMusic;
        sfxVolumeSlider.value = savedSFX;

        // Hook up listeners
        masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChange);
        musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChange);
        sfxVolumeSlider.onValueChanged.AddListener(OnSFXVolumeChange);
        resolutionDropdown.onValueChanged.AddListener(OnResolutionChange);
        qualityDropdown.onValueChanged.AddListener(OnQualityChange);
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

    public void OnMusicVolumeChange(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetMusicVolume(value);
        PlayerPrefs.SetFloat("MusicVol", value);
    }

    public void OnSFXVolumeChange(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetSFXVolume(value);
        PlayerPrefs.SetFloat("SFXVol", value);
    }

    // =============================
    // GRAPHICS CONTROLS
    // =============================
    public void OnResolutionChange(int index)
    {
        ApplyResolution(index);
        PlayerPrefs.SetInt("ResolutionIndex", index);
        Debug.Log($"[SettingsManager] Resolution changed to {resolutions[index].width}x{resolutions[index].height}");
    }

    public void OnQualityChange(int index)
    {
        ApplyQuality(index);
        PlayerPrefs.SetInt("QualityIndex", index);
        Debug.Log($"[SettingsManager] Quality changed to {QualitySettings.names[index]}");
    }

    public void OnFullscreenToggle(bool isFullscreen)
    {
        ApplyFullscreen(isFullscreen);
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
        Debug.Log($"[SettingsManager] Fullscreen set to: {isFullscreen}");
    }

    // =============================
    // APPLY METHODS
    // =============================
    private void ApplyResolution(int index)
    {
        if (index < 0 || index >= resolutions.Length) return;
        Resolution res = resolutions[index];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen);
        Debug.Log($"[SettingsManager] Applying resolution: {res.width}x{res.height}");
    }

    private void ApplyQuality(int index)
    {
        if (index < 0 || index >= QualitySettings.names.Length) return;
        QualitySettings.SetQualityLevel(index, true);
        Debug.Log($"[SettingsManager] Applying quality level: {QualitySettings.names[index]}");
    }

    private void ApplyFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        Debug.Log($"[SettingsManager] Applying fullscreen: {isFullscreen}");
    }

    private void OnDestroy()
    {
        PlayerPrefs.Save();
    }
}
