using UnityEngine;
using UnityEngine.SceneManagement;

public class LooseTransition : MonoBehaviour
{
    [Header("Scene Settings")]
    [Tooltip("Name of the scene to load when the player loses.")]
    public string loseSceneName = "LooseScene";

    [Header("Check Settings")]
    [Tooltip("How often (in seconds) to check the cow amount.")]
    public float checkInterval = 1f;

    private float _timer;

    private void Update()
    {
        _timer += Time.deltaTime;

        if (_timer >= checkInterval)
        {
            _timer = 0f;
            CheckCowAmount();
        }
    }

    private void CheckCowAmount()
    {
        var saveSystem = GameManager_Singleton.Instance.GetComponent<GameManager_SaveSystem>();

        if (saveSystem == null)
        {
            Debug.LogWarning("LooseTransition: SaveSystem not found on GameManager_Singleton!");
            return;
        }

        if (saveSystem.PlayerData_Curr == null)
        {
            Debug.LogWarning("LooseTransition: PlayerData_Curr is null!");
            return;
        }

        int cowsLeft = saveSystem.PlayerData_Curr.CowAmount;

        if (cowsLeft <= 0)
        {
            TriggerLoseScene();
        }
    }

    private void TriggerLoseScene()
    {
        Debug.Log("LooseTransition: All cows lost. Transitioning to lose scene...");
        SceneManager.LoadScene(loseSceneName);
    }
}
