using System.Collections.Generic;
using UnityEngine;

public class WaveGenerator : MonoBehaviour
{


    // a wave is a queue of entities to spawn

    GameManager_NightCounter nightCounter;


    private void Awake()
    {
        nightCounter = GameManager_Singleton.Instance.GetComponent<GameManager_NightCounter>();
    }


    public void GenerateWaveData_ForNight(int nightNumber)
    {

        // for now, just a placeholder
        Debug.Log($"Generating wave data for night {nightNumber}");

        /*
         
         


         
         
         
         
         
         
         
         
         */


    }



}



public struct WaveData
{

    // supposed to be waveEntity type, string for now.
    List<string> entitySpawnOrder_Names;

    List<int> entitySpawnOrder_Amount;

    List<float> delayOrder;


    public WaveData(List<string> names, List<int> amounts, List<float> delays)
    {
        entitySpawnOrder_Names = new List<string>();
        entitySpawnOrder_Amount = new List<int>();
        delayOrder = new List<float>();
    }

}


[CreateAssetMenu(fileName = "SO_WaveData_New", menuName = "ScriptableObjects/SO_WaveData")]
public class SO_WaveData : ScriptableObject
{

    public List<string> entitySpawnOrder_Names;

    public List<int> entitySpawnOrder_Amount;

    public List<float> delayOrder;


    public WaveData ToWaveData()
    {
        return new WaveData(entitySpawnOrder_Names, entitySpawnOrder_Amount, delayOrder);
    }
}