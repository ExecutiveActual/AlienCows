using UnityEngine;

public class GameManager_Singleton : MonoBehaviour
{
    public static GameManager_Singleton Instance { get; private set; }


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;

        // Optional: Survive scene changes
        DontDestroyOnLoad(this.gameObject);
    }


    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
