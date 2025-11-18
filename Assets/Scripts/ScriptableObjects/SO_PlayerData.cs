using UnityEngine;

[CreateAssetMenu(fileName = "SO_PlayerData", menuName = "Scriptable Objects/SO_PlayerData")]
public class SO_PlayerData : ScriptableObject
{
    public int NightNumber;

    public int MoneyAmount;

    public int CowAmount;

    public int[] UnlockedItems;

    public int[] SavedHotbar;


    public SO_PlayerData(int nightNumber, int moneyAmount, int cowAmount, int[] unlockedItems, int[] savedHotbar)
    {
        NightNumber = nightNumber;
        MoneyAmount = moneyAmount;
        CowAmount = cowAmount;
        UnlockedItems = unlockedItems;
        SavedHotbar = savedHotbar;
    }

    public PlayerData ToPlayerDataClass()
    {
        return new PlayerData(NightNumber, MoneyAmount, CowAmount, UnlockedItems, SavedHotbar);
    }

    public void FromPlayerDataClass(PlayerData data)
    {
        NightNumber = data.NightNumber;
        MoneyAmount = data.MoneyAmount;
        CowAmount = data.CowAmount;
        UnlockedItems = data.UnlockedItems;
        SavedHotbar = data.SavedHotbar;
    }

}

public class PlayerData
{
    public int NightNumber;
    public int MoneyAmount;
    public int CowAmount;
    public int[] UnlockedItems;
    public int[] SavedHotbar;

    public PlayerData()
    {

        NightNumber = 0;
        MoneyAmount = 5;
        CowAmount = 3;
        UnlockedItems = new int[] { 1 };
        SavedHotbar = new int[] { 1, 0, 0, 0, 0 };


        //PlayerData(0, 5, 3, new int[] { 1 }, new int[] { 1, 0, 0, 0, 0 });
    }

    public PlayerData(int nightNumber, int moneyAmount, int cowAmount, int[] unlockedItems, int[] savedHotbar)
    {
        NightNumber = nightNumber;
        MoneyAmount = moneyAmount;
        CowAmount = cowAmount;
        UnlockedItems = unlockedItems;
        SavedHotbar = savedHotbar;
    }
}