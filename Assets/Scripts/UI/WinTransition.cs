using UnityEngine;
using UnityEngine.SceneManagement;

public class WinTransition : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Minimum night number required to trigger Win Scene.")]
    [SerializeField] private int nightLimit = 10;

    [Tooltip("Name of the Win Scene to load.")]
    [SerializeField] private string winSceneName = "WinScene";

    [Tooltip("Delay before transition (for fade or dramatic effect).")]
    [SerializeField] private float transitionDelay = 3f;

    private GameManager_NightCounter gmNightCounterInstance;
    private GameManager_SceneChangeEvents gmSceneChangeEvents;

    private bool winTriggered = false;

    private void Start()
    {
        // Get references to global managers
        gmNightCounterInstance = GameManager_Singleton.Instance.GetComponent<GameManager_NightCounter>();
        gmSceneChangeEvents = GameManager_Singleton.Instance.GetComponent<GameManager_SceneChangeEvents>();

        // Run check at scene start
        CheckForWinCondition();
    }

    private void CheckForWinCondition()
    {
        if (gmNightCounterInstance == null)
        {
            Debug.LogError("WinTransition: NightCounter instance not found!");
            return;
        }

        int currentNight = gmNightCounterInstance.GetNightNumberCurrent();
        Debug.Log($"[WinTransition] Current Night: {currentNight}");

        if (currentNight >= nightLimit && !winTriggered)
        {
            winTriggered = true;
            Debug.Log("[WinTransition] Win condition met! Transitioning to Win Scene...");
            StartCoroutine(LoadWinSceneAfterDelay());
        }
    }

    private System.Collections.IEnumerator LoadWinSceneAfterDelay()
    {
        yield return new WaitForSeconds(transitionDelay);

        // Optional fade out can be triggered here later
        gmSceneChangeEvents.ChangeScene(winSceneName);
    }
}
