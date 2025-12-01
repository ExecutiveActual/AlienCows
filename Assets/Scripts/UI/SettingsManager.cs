using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SettingsManager : MonoBehaviour
{
    public Dropdown resolutionDropdown;
    public Toggle fullscreenToggle;
    public CanvasScaler canvasScaler;

    private Resolution[] customResolutions;

    private void Awake()
    {
        if (resolutionDropdown != null) resolutionDropdown.onValueChanged.RemoveAllListeners();
        if (fullscreenToggle != null) fullscreenToggle.onValueChanged.RemoveAllListeners();
    }

    private void Start()
    {
        customResolutions = new Resolution[3];
        customResolutions[0] = new Resolution { width = 2560, height = 1440 };
        customResolutions[1] = new Resolution { width = 1920, height = 1080 };
        customResolutions[2] = new Resolution { width = 1280, height = 720 };

        if (resolutionDropdown != null)
        {
            resolutionDropdown.ClearOptions();
            List<string> options = new List<string>();
            foreach (var res in customResolutions)
                options.Add($"{res.width} x {res.height}");
            resolutionDropdown.AddOptions(options);
        }

        int savedRes = PlayerPrefs.GetInt("ResolutionIndex", 0);
        bool savedFullscreen = PlayerPrefs.GetInt("Fullscreen", Screen.fullScreen ? 1 : 0) == 1;

        if (savedRes < 0 || savedRes >= customResolutions.Length) savedRes = 0;

        ApplyResolution(savedRes);
        ApplyFullscreen(savedFullscreen);
        UpdateCanvasScaler();

        if (resolutionDropdown != null) resolutionDropdown.value = savedRes;
        if (fullscreenToggle != null) fullscreenToggle.isOn = savedFullscreen;

        if (resolutionDropdown != null)
            resolutionDropdown.onValueChanged.AddListener(OnResolutionChange);

        if (fullscreenToggle != null)
            fullscreenToggle.onValueChanged.AddListener(OnFullscreenToggle);
    }

    public void OnResolutionChange(int index)
    {
        ApplyResolution(index);
        UpdateCanvasScaler();
        PlayerPrefs.SetInt("ResolutionIndex", index);
    }

    public void OnFullscreenToggle(bool isFullscreen)
    {
        ApplyFullscreen(isFullscreen);
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
    }

    private void ApplyResolution(int index)
    {
        if (index < 0 || index >= customResolutions.Length) return;
        Resolution res = customResolutions[index];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen);
    }

    private void ApplyFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    private void UpdateCanvasScaler()
    {
        if (canvasScaler == null) return;
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(2560f, 1440f);
        canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        canvasScaler.matchWidthOrHeight = 0.5f;
    }

    private void OnDestroy()
    {
        PlayerPrefs.Save();
    }
}
