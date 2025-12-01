using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class PauseMenuManager : MonoBehaviour
{
    public GameObject pauseMenuPanel;
    public string mainMenuSceneName;

    private bool isPaused = false;

    private void Start()
    {
        if (pauseMenuPanel) pauseMenuPanel.SetActive(false);
    }

    private void Update()
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
        if (pauseMenuPanel) pauseMenuPanel.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;

        if (GameManager_Singleton.Instance != null)
        {
            var uiManager = GameManager_Singleton.Instance.GetComponent<GameManager_UI>();
            uiManager?.UE_OnSwitchControlMode_UI?.Invoke();
        }

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
        if (pauseMenuPanel) pauseMenuPanel.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;

        if (GameManager_Singleton.Instance != null)
        {
            var uiManager = GameManager_Singleton.Instance.GetComponent<GameManager_UI>();
            uiManager?.UE_OnSwitchControlMode_Player?.Invoke();
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void ExitToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
