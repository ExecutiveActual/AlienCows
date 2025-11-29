using UnityEngine;

public class UpdatePlayerKillCount : MonoBehaviour
{
    



    public void UpdateKillCountOnDeath()
    {
        GameManager_Singleton.Instance.GetComponent<GameManager_SaveSystem>().PlayerData_Curr.KillCount += 1;
    }

}
