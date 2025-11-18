using UnityEngine;

public class GameManager_NightCounter : MonoBehaviour, IGameManagerModule
{
    GameManager_SaveSystem saveSystemInstance;


    public void OnInitializeModule()
    {

        saveSystemInstance = GameManager_Singleton.Instance.GetComponent<GameManager_SaveSystem>();

    }



    /// <summary>
    /// ONLY USE THROUGH THE START NEXT NIGHT BUTTON IN THE LOADOUT MENU!
    /// </summary>
    public void AdvanceToNextNight()
    {
        saveSystemInstance.PlayerData_Curr.NightNumber ++;
    }


    public void NightSceneManagerCheckIn(NightSceneManager nightSceneManager)
    {
        if (saveSystemInstance.PlayerData_Curr.NightNumber == 0)
        {
            saveSystemInstance.PlayerData_Curr.NightNumber = 1;
        }
        

        nightSceneManager.SetNightNumber(saveSystemInstance.PlayerData_Curr.NightNumber);

    }

    
}
