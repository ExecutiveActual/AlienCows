using UnityEngine;

public class GameManager_MoneyManager : MonoBehaviour, IGameManagerModule, IGM_SaveData
{


    public int CurrentMoney { get; private set; } = 0;

    protected SO_PlayerData currentPlayerSaveData;


    GameManager_SaveSystem saveSystemInstance;


    public void OnInitializeModule()
    {

        saveSystemInstance = GameManager_Singleton.Instance.GetComponent<GameManager_SaveSystem>();


        currentPlayerSaveData = saveSystemInstance.PlayerData_Curr;



        //ReadFromSaveData(currentPlayerSaveData);

    }


    private void Start()
    {
        ReadFromSaveData(currentPlayerSaveData);

        Debug.Log($"Money Manager initialized. Current money: {CurrentMoney}");
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            AddMoney(100);
            Debug.Log($"Added 100 money. Current money: {CurrentMoney}");
        }
    }


    public void AddMoney(int amount)
    {
        CurrentMoney += amount;
    }

    public bool SpendMoney(int amount)
    {
        if (CurrentMoney >= amount)
        {
            CurrentMoney -= amount;
            return true;
        }
        return false;
    }

    public void RemoveMoney(int amount)
    {
        CurrentMoney -= amount;
        if (CurrentMoney < 0) CurrentMoney = 0;
    }




    public void ReadFromSaveData(SO_PlayerData data)
    {
        CurrentMoney = data.MoneyAmount;
    }

    public void WriteToSaveData(SO_PlayerData data)
    {
        data.MoneyAmount = CurrentMoney;
    }


    private void OnApplicationQuit()
    {
        WriteToSaveData(currentPlayerSaveData);
    }
}
