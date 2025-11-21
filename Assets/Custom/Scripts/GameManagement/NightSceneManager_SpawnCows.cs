using System.Collections.Generic;
using UnityEngine;

public class NightSceneManager_SpawnCows : MonoBehaviour
{



    [SerializeField] private List<Transform> cowSpawnPoints;


    public void SpawnCows(int amount)
    {


        Debug.Log($"Spawned {amount} cows");


    }

}
