using UnityEngine;

/// <summary>
/// A lightweight global runner that can safely start coroutines
/// even after other GameObjects are destroyed.
/// </summary>
public class CoroutineRunner : MonoBehaviour
{
    private static CoroutineRunner _instance;

    public static CoroutineRunner Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject runnerObj = new GameObject("[CoroutineRunner]");
                DontDestroyOnLoad(runnerObj);
                _instance = runnerObj.AddComponent<CoroutineRunner>();
            }
            return _instance;
        }
    }
}
