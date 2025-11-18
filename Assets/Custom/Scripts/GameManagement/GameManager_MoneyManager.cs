using UnityEngine;

public class GameManager_MoneyManager : MonoBehaviour, IGameManagerModule, IGM_SaveData
{



    GameManager_SaveSystem saveSystemInstance;


    public void OnInitializeModule()
    {

        saveSystemInstance = GameManager_Singleton.Instance.GetComponent<GameManager_SaveSystem>();

        //saveSystemInstance.UE_OnUpdateSaveData.AddListener(UpdateSaveData);

        //ReadFromSaveData(currentPlayerSaveData);

    }


    private void Start()
    {
        ReadFromSaveData(saveSystemInstance.PlayerData_Curr);

        Debug.Log($"Money Manager initialized. Current money: {saveSystemInstance.PlayerData_Curr.MoneyAmount}");
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            AddMoney(100);
            Debug.Log($"Added 100 money. Current money: {saveSystemInstance.PlayerData_Curr.MoneyAmount}");
        }
    }


    private void UpdateSaveData()
    {
        ReadFromSaveData(saveSystemInstance.PlayerData_Curr);
    }


    public void AddMoney(int amount)
    {
        saveSystemInstance.PlayerData_Curr.MoneyAmount += amount;
    }

    public bool SpendMoney(int amount)
    {
        if (saveSystemInstance.PlayerData_Curr.MoneyAmount >= amount)
        {
            saveSystemInstance.PlayerData_Curr.MoneyAmount -= amount;
            return true;
        }
        return false;
    }

    public void RemoveMoney(int amount)
    {
        saveSystemInstance.PlayerData_Curr.MoneyAmount -= amount;
        if (saveSystemInstance.PlayerData_Curr.MoneyAmount < 0) saveSystemInstance.PlayerData_Curr.MoneyAmount = 0;
    }




    public void ReadFromSaveData(SO_PlayerData data)
    {
        saveSystemInstance.PlayerData_Curr.MoneyAmount = data.MoneyAmount;
    }

    public void WriteToSaveData(SO_PlayerData data)
    {
        data.MoneyAmount = saveSystemInstance.PlayerData_Curr.MoneyAmount;
    }


    private void OnApplicationQuit()
    {
        WriteToSaveData(saveSystemInstance.PlayerData_Curr);
    }
}
