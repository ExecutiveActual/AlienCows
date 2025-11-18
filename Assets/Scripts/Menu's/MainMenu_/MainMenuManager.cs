using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Menu Sections")]
    public GameObject mainMenuManager;
    public GameObject startGameManager;
    public GameObject settingsGameManager;
    public GameObject aboutGameManager;

    [Header("Start Game Panels")]
    public GameObject newGameConfirmation;   // you have this
    public GameObject loadGameManager;       // you have this

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Audio Clips")]
    public AudioClip backgroundMusic;
    public AudioClip buttonHoverSound;
    public AudioClip buttonClickSound;

    [Header("Volume Controls")]
    [Range(0f, 1f)] public float musicVolume = 1f;
    [Range(0f, 1f)] public float clickVolume = 1f;
    [Range(0f, 1f)] public float hoverVolume = 1f;

    [Header("Scene Settings")]
    public string newGameSceneName;



    private GameManager_SaveSystem saveSystemInstance;


    void Start()
    {

        saveSystemInstance = GameManager_Singleton.Instance.GetComponent<GameManager_SaveSystem>();

        EnableOnly(mainMenuManager);

        // hide panels on start
        if (newGameConfirmation) newGameConfirmation.SetActive(false);
        if (loadGameManager) loadGameManager.SetActive(false);

        PlayBackgroundMusic();
    }

    void EnableOnly(GameObject target)
    {
        mainMenuManager.SetActive(target == mainMenuManager);
        startGameManager.SetActive(target == startGameManager);
        settingsGameManager.SetActive(target == settingsGameManager);
        aboutGameManager.SetActive(target == aboutGameManager);
    }

    //---------------------------
    // MAIN MENU
    //---------------------------

    public void OnStartGamePressed()
    {
        PlayClickSound();
        EnableOnly(startGameManager);

        newGameConfirmation.SetActive(false);
        loadGameManager.SetActive(false);
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

    public void OnBackToMainMenuPressed()
    {
        PlayClickSound();
        EnableOnly(mainMenuManager);
    }

    //---------------------------
    // START GAME OPTIONS
    //---------------------------

    public void OnNewGamePressed()
    {
        PlayClickSound();
        startGameManager.SetActive(false);
        newGameConfirmation.SetActive(true);
    }

    public void OnNewGameYesPressed()
    {
        PlayClickSound();

        saveSystemInstance.WipeSaveGame();

        SceneManager.LoadScene(newGameSceneName);
    }

    public void OnNewGameNoPressed()
    {
        PlayClickSound();
        newGameConfirmation.SetActive(false);
        startGameManager.SetActive(true);
    }

    public void OnLoadGamePressed()
    {
        PlayClickSound();
        startGameManager.SetActive(false);
        loadGameManager.SetActive(true);
    }

    public void OnLoadGameYesPressed()
    {
        PlayClickSound();
        Debug.Log("Loading last saved game...");

        SceneManager.LoadScene(newGameSceneName);

    }

    public void OnLoadGameNoPressed()
    {
        PlayClickSound();
        loadGameManager.SetActive(false);
        startGameManager.SetActive(true);
    }

    //---------------------------
    // AUDIO
    //---------------------------

    public void PlayHoverSound()
    {
        if (buttonHoverSound && sfxSource)
            sfxSource.PlayOneShot(buttonHoverSound, hoverVolume);
    }

    void PlayClickSound()
    {
        if (buttonClickSound && sfxSource)
            sfxSource.PlayOneShot(buttonClickSound, clickVolume);
    }

    void PlayBackgroundMusic()
    {
        if (musicSource && backgroundMusic)
        {
            musicSource.clip = backgroundMusic;
            musicSource.volume = musicVolume;
            musicSource.loop = true;
            musicSource.Play();
        }
    }
}
