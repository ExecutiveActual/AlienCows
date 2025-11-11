using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PauseMenuManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject pauseMenuPanel;
    public GameObject settingsGameManager;
    public GameObject viewCollectionsManager;

    [Header("Main Menu Scene")]
    public string mainMenuSceneName;

    [Header("Audio")]
    public AudioSource uiAudioSource;
    public AudioClip hoverSound;
    public AudioClip clickSound;

    private bool isPaused = false;

    void Start()
    {
        pauseMenuPanel.SetActive(false);
        settingsGameManager.SetActive(false);
        viewCollectionsManager.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ResumeGame();
            else
                OpenPauseMenu();
        }
    }

    public void OpenPauseMenu()
    {
        pauseMenuPanel.SetActive(true);
        settingsGameManager.SetActive(false);
        viewCollectionsManager.SetActive(false);
        Time.timeScale = 0f;
        isPaused = true;

        GameManager_Singleton.Instance
            .GetComponent<GameManager_UI>()
            .UE_OnNightEnd?.Invoke();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (EventSystem.current == null)
        {
            GameObject es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            DontDestroyOnLoad(es);
        }
    }

    public void ResumeGame()
    {
        pauseMenuPanel.SetActive(false);
        settingsGameManager.SetActive(false);
        viewCollectionsManager.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;

        GameManager_Singleton.Instance
            .GetComponent<GameManager_UI>()
            .UE_OnReturnControlToPlayer?.Invoke();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void OpenSettings()
    {
        PlayClickSound();
        pauseMenuPanel.SetActive(false);
        settingsGameManager.SetActive(true);
    }

    public void OpenViewCollections()
    {
        PlayClickSound();
        pauseMenuPanel.SetActive(false);
        viewCollectionsManager.SetActive(true);
    }

    public void BackToPauseMenuFromSettings()
    {
        PlayClickSound();
        settingsGameManager.SetActive(false);
        pauseMenuPanel.SetActive(true);
    }

    public void BackToPauseMenuFromCollections()
    {
        PlayClickSound();
        viewCollectionsManager.SetActive(false);
        pauseMenuPanel.SetActive(true);
    }

    public void ExitToMainMenu()
    {
        PlayClickSound();
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void PlayHoverSound()
    {
        if (uiAudioSource && hoverSound)
            uiAudioSource.PlayOneShot(hoverSound);
    }

    public void PlayClickSound()
    {
        if (uiAudioSource && clickSound)
            uiAudioSource.PlayOneShot(clickSound);
    }
}
