using UnityEngine;
using System.IO;
using System;
using UnityEngine.Events;


public class GameManager_SaveSystem : MonoBehaviour, IGameManagerModule
{

    public SO_PlayerData PlayerData_Curr { get; private set; }

    private string saveFileName = "player_save_data.json";


    GameManager_NightCounter gm_NightCounter;



    //public UnityEvent UE_OnUpdateSaveData;




    private void Awake()
    {
        PlayerData_Curr = ScriptableObject.CreateInstance<SO_PlayerData>();

        PlayerData newPlayerData = new PlayerData();
        PlayerData_Curr.FromPlayerDataClass(newPlayerData);

        if (!LoadFromFile(saveFileName))
        {
            
            SaveToFile(saveFileName);
            Debug.Log("No save file found. Created new save data.");
        }
        else
        {
            Debug.Log("Save file loaded successfully.");
        }

        UpdateSaveData();
    }



    public void OnInitializeModule()
    {
        gm_NightCounter = GameManager_Singleton.Instance.GetComponent<GameManager_NightCounter>();
    }


    public bool IsNewGame()
    {

        if (PlayerData_Curr.NightNumber == 0)
        {
            return true;
        }
        else
        {
            return false;
        }

    }


    private void UpdateSaveData()
    {
        //UE_OnUpdateSaveData.Invoke();
    }


    private string GetSavePath(string fileName)
    {
        return Path.Combine(Application.persistentDataPath, fileName);
    }


    public void WipeSaveGame()
    {
        // Option A: Delete the file entirely (recommended - guarantees fresh start)
        string path = GetSavePath(saveFileName);
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log("Save file deleted: " + path);
        }

        // Reset in-memory data
        PlayerData_Curr = ScriptableObject.CreateInstance<SO_PlayerData>();
        PlayerData newPlayerData = new PlayerData();
        PlayerData_Curr.FromPlayerDataClass(newPlayerData);

        // Optionally save the fresh data immediately
        SaveToFile(saveFileName);

        UpdateSaveData();

        Debug.Log("Save game has been completely wiped and reset!");
    }

    private void SaveToFile(string fileName)
    {
        string path = GetSavePath(fileName);
        string jsonData = JsonUtility.ToJson(PlayerData_Curr.ToPlayerDataClass(), true);
        File.WriteAllText(path, jsonData);
        //Debug.Log("Game saved to: " + path);
    }

    private bool LoadFromFile(string fileName)
    {
        string path = GetSavePath(fileName);
        if (File.Exists(path))
        {
            string jsonData = File.ReadAllText(path);
            PlayerData loaded = JsonUtility.FromJson<PlayerData>(jsonData);
            if (loaded != null)
            {
                PlayerData_Curr.FromPlayerDataClass(loaded);
                UpdateSaveData();
                //Debug.Log("Save file loaded from: " + path);
                return true;
            }
        }

        Debug.LogWarning("No save file found at: " + path);
        return false;
    }

    private void OnApplicationQuit()
    {
        SaveToFile(saveFileName);
    }

}
