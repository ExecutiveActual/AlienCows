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
        // Make sure pause panels start hidden
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

    // ---------- CORE PAUSE LOGIC ----------
    public void OpenPauseMenu()
    {
        pauseMenuPanel.SetActive(true);
        settingsGameManager.SetActive(false);
        viewCollectionsManager.SetActive(false);
        Time.timeScale = 0f;
        isPaused = true;

        // Call the GameManager event for night end (pause)
        GameManager_Singleton.Instance
            .GetComponent<GameManager_UI>()
            .UE_OnNightEnd?.Invoke();

        // Unlock cursor for UI interaction
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Ensure EventSystem exists so buttons work
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

        // Call the GameManager event for returning control
        GameManager_Singleton.Instance
            .GetComponent<GameManager_UI>()
            .UE_OnReturnControlToPlayer?.Invoke();

        // Lock cursor back to center for gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // ---------- MENU NAVIGATION ----------
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
        Time.timeScale = 1f; // Reset before scene switch
        SceneManager.LoadScene(mainMenuSceneName);
    }

    // ---------- UI SOUND HANDLERS ----------
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
