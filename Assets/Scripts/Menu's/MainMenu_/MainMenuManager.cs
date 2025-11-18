using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Menu Sections")]
    public GameObject mainMenuManager;
    public GameObject startGameManager;
    public GameObject settingsGameManager;
    public GameObject aboutGameManager;

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Audio Clips")]
    public AudioClip backgroundMusic;
    public AudioClip buttonHoverSound;
    public AudioClip buttonClickSound;

    [Header("Volume Controls")]
    [Range(0f, 1f)] public float musicVolume = 1f;   // background music
    [Range(0f, 1f)] public float clickVolume = 1f;   // click sound
    [Range(0f, 1f)] public float hoverVolume = 1f;   // hover sound

    [Header("Scene Settings")]
    public string newGameSceneName;

    void Start()
    {
        EnableOnly(mainMenuManager);
        PlayBackgroundMusic();
    }

    void EnableOnly(GameObject target)
    {
        mainMenuManager.SetActive(target == mainMenuManager);
        startGameManager.SetActive(target == startGameManager);
        settingsGameManager.SetActive(target == settingsGameManager);
        aboutGameManager.SetActive(target == aboutGameManager);
    }

    // --- Main Menu ---

    public void OnStartGamePressed()
    {
        PlayClickSound();
        EnableOnly(startGameManager);
    }

    public void OnSettingsPressed()
    {
        PlayClickSound();
        EnableOnly(settingsGameManager);
    }

    public void OnAboutPressed()
    {
        PlayClickSound();
        EnableOnly(aboutGameManager);
    }

    public void OnExitGamePressed()
    {
        PlayClickSound();
        Application.Quit();
    }

    // --- Sub Menu ---

    public void OnBackToMainMenuPressed()
    {
        PlayClickSound();
        EnableOnly(mainMenuManager);
    }

    public void OnNewGamePressed()
    {
        PlayClickSound();
        if (!string.IsNullOrEmpty(newGameSceneName))
            SceneManager.LoadScene(newGameSceneName);
        else
            Debug.LogWarning("Scene name not assigned in inspector.");
    }

    // --- Audio ---

    public void PlayHoverSound()
    {
        if (buttonHoverSound != null && sfxSource != null)
            sfxSource.PlayOneShot(buttonHoverSound, hoverVolume);
    }

    void PlayClickSound()
    {
        if (buttonClickSound != null && sfxSource != null)
            sfxSource.PlayOneShot(buttonClickSound, clickVolume);
    }

    void PlayBackgroundMusic()
    {
        if (musicSource != null && backgroundMusic != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.volume = musicVolume;
            musicSource.loop = true;
            musicSource.Play();
        }
    }
}
