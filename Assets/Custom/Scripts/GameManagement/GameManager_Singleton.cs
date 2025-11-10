using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameManager_Singleton : MonoBehaviour
{
    public static GameManager_Singleton Instance { get; private set; }


    private List<IGameManagerModule> gm_modules = new List<IGameManagerModule>();


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


        GetComponents<IGameManagerModule>(gm_modules);

        InitializeGameManagerModules();

    }



    private void InitializeGameManagerModules()
    {

        foreach (IGameManagerModule module in gm_modules)
        {
            module.OnInitializeModule();
        }

    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
