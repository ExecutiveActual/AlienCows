using UnityEngine;

[CreateAssetMenu(fileName = "SO_PlayerData", menuName = "Scriptable Objects/SO_PlayerData")]
public class SO_PlayerData : ScriptableObject
{
    public int NightNumber;

    public int MoneyAmount;

    public int CowAmount;

    public int[] UnlockedItems;


    public SO_PlayerData(int nightNumber, int moneyAmount, int cowAmount, int[] unlockedItems)
    {
        NightNumber = nightNumber;
        MoneyAmount = moneyAmount;
        CowAmount = cowAmount;
        UnlockedItems = unlockedItems;
    }

    public PlayerData ToPlayerDataClass()
    {
        return new PlayerData(NightNumber, MoneyAmount, CowAmount, UnlockedItems);
    }

    public void FromPlayerDataClass(PlayerData data)
    {
        NightNumber = data.NightNumber;
        MoneyAmount = data.MoneyAmount;
        CowAmount = data.CowAmount;
        UnlockedItems = data.UnlockedItems;
    }

}
