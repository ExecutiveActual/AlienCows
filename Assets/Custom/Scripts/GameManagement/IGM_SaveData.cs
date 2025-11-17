using UnityEngine;

public interface IGM_SaveData
{

    public abstract void ReadFromSaveData(SO_PlayerData data);

    public abstract void WriteToSaveData(SO_PlayerData data);



}
