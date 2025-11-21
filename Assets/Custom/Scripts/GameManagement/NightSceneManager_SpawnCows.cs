using System.Collections.Generic;
using UnityEngine;

public class NightSceneManager_SpawnCows : MonoBehaviour
{


    [SerializeField] private GameObject cowPrefab;

    [SerializeField] private Transform SpawnPointParent;

    [SerializeField] private float spawnRandomSpread = 5f;

    private Transform[] cowSpawnPoints;



    private void Awake()
    {
        cowSpawnPoints = SpawnPointParent.GetComponentsInChildren<Transform>();
    }


    public void SpawnCows(int amount)
    {

        for (int i = 0; i < amount; i++)
        {

            Transform spawnPoint = GetRandomSpawnPoint();
            Instantiate(cowPrefab, GetRandomPos(spawnPoint.position, spawnRandomSpread), GetRandomRot());

            Debug.Log($"spawned Cow i = {i}");
        }

        Debug.Log($"Spawned {amount} cows");


    }

    private Transform GetRandomSpawnPoint()
    {
        List<Transform> possibleSpawnPoints = new List<Transform>(cowSpawnPoints);
        int randomIndex = Random.Range(0, possibleSpawnPoints.Count);
        return possibleSpawnPoints[randomIndex];
    }


    private Vector3 GetRandomPos(Vector3 originalPos, float spread)
    {
        Vector3 pos = originalPos;
        pos.x += Random.Range(-spread, spread);
        pos.z += Random.Range(-spread, spread);
        return pos;
    }

    private Quaternion GetRandomRot()
    {
        return Quaternion.Euler(0, Random.Range(0, 360), 0);
    }


    private void OnDrawGizmos()
    {
        if (SpawnPointParent == null) return;
        Transform[] spawnPoints = SpawnPointParent.GetComponentsInChildren<Transform>();
        Gizmos.color = Color.green;
        foreach (Transform spawnPoint in spawnPoints)
        {
            Gizmos.DrawSphere(spawnPoint.position, 0.2f);
        }
    }

}
