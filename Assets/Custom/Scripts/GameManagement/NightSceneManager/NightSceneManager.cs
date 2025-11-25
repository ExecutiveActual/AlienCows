using System.Collections;
using UnityEngine;

public class NightSceneManager : MonoBehaviour
{


    GameManager_NightCounter gm_NightCounterInstance;

    GameManager_SaveSystem gm_SaveSystemInstance;



    private void Start()
    {

        gm_SaveSystemInstance = GameManager_Singleton.Instance.GetComponent<GameManager_SaveSystem>();

        gm_NightCounterInstance = GameManager_Singleton.Instance.GetComponent<GameManager_NightCounter>();

        gm_NightCounterInstance.NightSceneManagerCheckIn(this);


        Debug.Log("Night Scene Loaded!");
    }



    private void SpawnCows(int amount)
    {
        GetComponent<NightSceneManager_SpawnCows>().SpawnCows(amount);
    }

    public void SetNightNumber(int nightNumber)
    {

        SpawnCows(gm_SaveSystemInstance.PlayerData_Curr.CowAmount);

    }


    public void OnAllWavesComplete()
    {

        Debug.Log("All Waves Complete!");


        //Check other shit

        StartCoroutine(WaitForTransition());


    }

    private IEnumerator WaitForTransition()
    {
        yield return new WaitForSeconds(3f);

        GameManager_Singleton.Instance.GetComponent<GameManager_SceneChangeEvents>().ChangeScene("Day Scene");
    }


}
