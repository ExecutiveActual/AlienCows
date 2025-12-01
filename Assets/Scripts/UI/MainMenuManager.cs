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
    public GameObject newGameConfirmation;
    public GameObject loadGameManager;

    [Header("Scene Settings")]
    public string newGameSceneName;

    private GameManager_SaveSystem saveSystemInstance;

    void Start()
    {
        saveSystemInstance = GameManager_Singleton.Instance.GetComponent<GameManager_SaveSystem>();
        EnableOnly(mainMenuManager);
        if (newGameConfirmation) newGameConfirmation.SetActive(false);
        if (loadGameManager) loadGameManager.SetActive(false);
    }

    void EnableOnly(GameObject target)
    {
        mainMenuManager.SetActive(target == mainMenuManager);
        startGameManager.SetActive(target == startGameManager);
        settingsGameManager.SetActive(target == settingsGameManager);
        aboutGameManager.SetActive(target == aboutGameManager);
    }

    public void OnStartGamePressed()
    {
        EnableOnly(startGameManager);
        newGameConfirmation.SetActive(false);
        loadGameManager.SetActive(false);
    }

    public void OnSettingsPressed()
    {
        EnableOnly(settingsGameManager);
    }

    public void OnAboutPressed()
    {
        EnableOnly(aboutGameManager);
    }

    public void OnCreditsPressed()
    {
        SceneManager.LoadScene("Credits");
    }

    public void OnExitGamePressed()
    {
        Application.Quit();
    }

    public void OnBackToMainMenuPressed()
    {
        EnableOnly(mainMenuManager);
    }

    public void OnNewGamePressed()
    {
        startGameManager.SetActive(false);
        newGameConfirmation.SetActive(true);
    }

    public void OnNewGameYesPressed()
    {
        saveSystemInstance.WipeSaveGame();
        SceneManager.LoadScene(newGameSceneName);
    }

    public void OnNewGameNoPressed()
    {
        newGameConfirmation.SetActive(false);
        startGameManager.SetActive(true);
    }

    public void OnLoadGamePressed()
    {
        startGameManager.SetActive(false);
        loadGameManager.SetActive(true);
    }

    public void OnLoadGameYesPressed()
    {
        SceneManager.LoadScene(newGameSceneName);
    }

    public void OnLoadGameNoPressed()
    {
        loadGameManager.SetActive(false);
        startGameManager.SetActive(true);
    }
}
