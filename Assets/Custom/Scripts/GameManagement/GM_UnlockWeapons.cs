using System;
using UnityEngine;

public class GM_UnlockWeapons : MonoBehaviour, IGameManagerModule
{

    GameManager_GiveInventory gm_GiveInventory;

    GameManager_NightCounter gm_NightCounter;

    GameManager_SceneChangeEvents gm_SceneChangeEvents;

    GameManager_SaveSystem gm_SaveSystem;


    public void OnInitializeModule()
    {
        gm_GiveInventory = GameManager_Singleton.Instance.GetComponent<GameManager_GiveInventory>();

        gm_NightCounter = GameManager_Singleton.Instance.GetComponent<GameManager_NightCounter>();

        gm_SceneChangeEvents = GameManager_Singleton.Instance.GetComponent<GameManager_SceneChangeEvents>();
        gm_SceneChangeEvents.UE_OnChangeScene.AddListener(OnSceneChange);

        gm_SaveSystem = GameManager_Singleton.Instance.GetComponent<GameManager_SaveSystem>();
    }

    private void OnSceneChange(string sceneName)
    {

        if (sceneName == "Day Scene")
        {
            Debug.Log("Daytime Unlock time!");

            if (gm_NightCounter.GetNightNumberCurrent() == 3)
            {
                UnlockWeaponByID(3); // REVOLVER UNLOCKED
            }

            if (gm_NightCounter.GetNightNumberCurrent() == 6)
            {
                UnlockWeaponByID(6); // RIFLE UNLOCKED
            }
        }

    }


    private void UnlockWeaponByID(int weaponID)
    {
        if (weaponID == 3)
        {
            gm_SaveSystem.PlayerData_Curr.SavedHotbar[1] = 3;
        }
        else if (weaponID == 6)
        {
            gm_SaveSystem.PlayerData_Curr.SavedHotbar[2] = 6;
        }

    }

}
