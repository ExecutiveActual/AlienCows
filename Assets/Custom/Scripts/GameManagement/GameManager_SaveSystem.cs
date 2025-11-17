using UnityEngine;
using System.IO;

public class GameManager_SaveSystem : MonoBehaviour, IGameManagerModule
{

    public SO_PlayerData PlayerData_Curr { get; private set; }

    private string saveFileName = "player_save_data.json";


    public bool IsNewGame = true;


    private void Awake()
    {

        PlayerData_Curr = ScriptableObject.CreateInstance<SO_PlayerData>();

        PlayerData newPlayerData = new PlayerData(1, 5, 3, new int[] { 1 });

        PlayerData_Curr.FromPlayerDataClass(newPlayerData);



        if (!LoadFromFile(saveFileName))
        {

            IsNewGame = true;

            SaveToFile(saveFileName);

            Debug.Log("No save file found. Created new save data.");
        }
        else
        {
            IsNewGame = false;
            Debug.Log("Save file loaded successfully.");
        }

    }



    public void OnInitializeModule()
    {

    }



    private void SaveToFile(string fileName)
    {
        string jsonData = JsonUtility.ToJson(PlayerData_Curr.ToPlayerDataClass());
        File.WriteAllText($"{Application.dataPath}/{fileName}", jsonData);
    }

    private bool LoadFromFile(string fileName)
    {
        if (File.Exists($"{Application.dataPath}/{fileName}"))
        {
            string jsonData = File.ReadAllText($"{Application.dataPath}/{fileName}");

            PlayerData_Curr.FromPlayerDataClass(JsonUtility.FromJson<PlayerData>(jsonData));
            return true;
        }
        else
        {
            Debug.LogWarning("Save file not found: " + $"{Application.dataPath}/{fileName}");
            return false;
        }
    }

    private void OnApplicationQuit()
    {
        SaveToFile(saveFileName);
    }

}


public class PlayerData
{
    public int NightNumber;
    public int MoneyAmount;
    public int CowAmount;
    public int[] UnlockedItems;

    public PlayerData(int nightNumber, int moneyAmount, int cowAmount, int[] unlockedItems)
    {
        NightNumber = nightNumber;
        MoneyAmount = moneyAmount;
        CowAmount = cowAmount;
        UnlockedItems = unlockedItems;
    }
}