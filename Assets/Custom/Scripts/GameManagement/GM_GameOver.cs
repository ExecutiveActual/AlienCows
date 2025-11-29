using System;
using UnityEngine;

public class GM_GameOver : MonoBehaviour, IGameManagerModule
{


    GameManager_SaveSystem gm_SaveSystem;


    public void OnInitializeModule()
    {
        gm_SaveSystem = GameManager_Singleton.Instance.GetComponent<GameManager_SaveSystem>();

        gm_SaveSystem.UE_OnUpdateSaveData.AddListener(OnUpdateSaveData);
    }

    private void OnDisable()
    {
        gm_SaveSystem.UE_OnUpdateSaveData.RemoveListener(OnUpdateSaveData);
    }

    private void OnUpdateSaveData()
    {

        // check if cows are zero and do game over stuff


    }
}
