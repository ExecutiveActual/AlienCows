using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Persistence")]
    [Tooltip("Keep this AudioManager alive across all scenes.")]
    public bool dontDestroyOnLoad = true;

    [Header("Audio Sources (Optional)")]
    public AudioSource bgmSource;
    public AudioSource sfxSource;
    public AudioSource uiSource;
    public AudioSource ambientSource;

    [Header("Default Clips (Optional)")]
    public AudioClip defaultBGM;
    public AudioClip defaultAmbient;

    [Header("SFX Library (Optional)")]
    public List<AudioClip> sfxClips = new List<AudioClip>();

    private Dictionary<string, AudioClip> sfxLookup = new Dictionary<string, AudioClip>();

    [Header("Volume Settings")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float musicVolume = 0.8f;
    [Range(0f, 1f)] public float sfxVolume = 0.8f;
    [Range(0f, 1f)] public float uiVolume = 0.8f;
    [Range(0f, 1f)] public float ambientVolume = 0.8f;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        if (dontDestroyOnLoad)
            DontDestroyOnLoad(gameObject);

        // Build lookup dictionary for SFX
        foreach (AudioClip clip in sfxClips)
        {
            if (clip != null && !sfxLookup.ContainsKey(clip.name))
                sfxLookup.Add(clip.name, clip);
        }

        LoadVolumes();
        ApplyVolumes();

        // Auto-play default bgm/ambient
        if (bgmSource && defaultBGM)
            PlayBGM(defaultBGM);

        if (ambientSource && defaultAmbient)
            PlayAmbient(defaultAmbient);
    }

    private void Update()
    {
        // Live update volumes if changed in inspector
        ApplyVolumes();
    }

    // =======================
    // Play Methods
    // =======================
    public void PlayBGM(AudioClip clip, bool loop = true)
    {
        if (bgmSource == null || clip == null) return;
        bgmSource.clip = clip;
        bgmSource.loop = loop;
        bgmSource.Play();
    }

    public void StopBGM()
    {
        if (bgmSource) bgmSource.Stop();
    }

    public void PlayAmbient(AudioClip clip, bool loop = true)
    {
        if (ambientSource == null || clip == null) return;
        ambientSource.clip = clip;
        ambientSource.loop = loop;
        ambientSource.Play();
    }

    public void StopAmbient()
    {
        if (ambientSource) ambientSource.Stop();
    }

    public void PlaySFX(string clipName, float volume = 1f)
    {
        if (sfxSource == null || !sfxLookup.ContainsKey(clipName)) return;
        sfxSource.PlayOneShot(sfxLookup[clipName], volume * sfxVolume * masterVolume);
    }

    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (sfxSource == null || clip == null) return;
        sfxSource.PlayOneShot(clip, volume * sfxVolume * masterVolume);
    }

    public void PlayUI(AudioClip clip, float volume = 1f)
    {
        if (uiSource == null || clip == null) return;
        uiSource.PlayOneShot(clip, volume * uiVolume * masterVolume);
    }

    // =======================
    // Volume Handling
    // =======================
    public void SetMasterVolume(float value)
    {
        masterVolume = Mathf.Clamp01(value);
        ApplyVolumes();
        SaveVolumes();
    }

    public void SetMusicVolume(float value)
    {
        musicVolume = Mathf.Clamp01(value);
        ApplyVolumes();
        SaveVolumes();
    }

    public void SetSFXVolume(float value)
    {
        sfxVolume = Mathf.Clamp01(value);
        SaveVolumes();
    }

    public void SetUIVolume(float value)
    {
        uiVolume = Mathf.Clamp01(value);
        SaveVolumes();
    }

    public void SetAmbientVolume(float value)
    {
        ambientVolume = Mathf.Clamp01(value);
        ApplyVolumes();
        SaveVolumes();
    }

    private void ApplyVolumes()
    {
        if (bgmSource) bgmSource.volume = masterVolume * musicVolume;
        if (sfxSource) sfxSource.volume = masterVolume * sfxVolume;
        if (uiSource) uiSource.volume = masterVolume * uiVolume;
        if (ambientSource) ambientSource.volume = masterVolume * ambientVolume;
    }

    // =======================
    // Save / Load Settings
    // =======================
    private void SaveVolumes()
    {
        PlayerPrefs.SetFloat("MasterVol", masterVolume);
        PlayerPrefs.SetFloat("MusicVol", musicVolume);
        PlayerPrefs.SetFloat("SFXVol", sfxVolume);
        PlayerPrefs.SetFloat("UIVol", uiVolume);
        PlayerPrefs.SetFloat("AmbientVol", ambientVolume);
        PlayerPrefs.Save();
    }

    private void LoadVolumes()
    {
        masterVolume = PlayerPrefs.GetFloat("MasterVol", masterVolume);
        musicVolume = PlayerPrefs.GetFloat("MusicVol", musicVolume);
        sfxVolume = PlayerPrefs.GetFloat("SFXVol", sfxVolume);
        uiVolume = PlayerPrefs.GetFloat("UIVol", uiVolume);
        ambientVolume = PlayerPrefs.GetFloat("AmbientVol", ambientVolume);
    }
}
