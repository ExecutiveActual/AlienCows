using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class UFOSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public Transform[] spawnPoints;
    public int totalUFOCount = 5;
    public float spawnDelay = 1.5f;

    [Header("Enemy Types")]
    public GameObject normalUFO;
    public GameObject droneUFO;
    public GameObject bossUFO;

    [Header("Stats Tracking")]
    public int moneyCount;
    public int ufoDown;
    public int ufoFleed;
    public int cowsLost;

    [Header("Events (For UI or Game Manager)")]
    public UnityEvent<int> OnMoneyUpdated;
    public UnityEvent<int> OnUFODownUpdated;
    public UnityEvent<int> OnUFOFleedUpdated;
    public UnityEvent<int> OnCowsLostUpdated;
    public UnityEvent OnAllUFODone;

    private int spawnedUFOs = 0;
    private int activeUFOs = 0;

    private void Start()
    {
        StartCoroutine(SpawnSequence());
    }

    private System.Collections.IEnumerator SpawnSequence()
    {
        while (spawnedUFOs < totalUFOCount)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(spawnDelay);
        }
    }

    private void SpawnEnemy()
    {
        if (spawnPoints.Length == 0) return;

        Transform spawn = spawnPoints[Random.Range(0, spawnPoints.Length)];

        GameObject prefabToSpawn = PickEnemyType();

        GameObject ufo = Instantiate(prefabToSpawn, spawn.position, spawn.rotation);

        // FIXED: right class name
        UFOController controller = ufo.GetComponent<UFOController>();

        controller.OnUFODestroyed.AddListener(HandleUFODestroyed);
        controller.OnUFOFleed.AddListener(HandleUFOFleed);
        controller.OnCowLost.AddListener(HandleCowLost);

        spawnedUFOs++;
        activeUFOs++;
    }

    private GameObject PickEnemyType()
    {
        float r = Random.value;

        // You can fine tune these % in inspector later by converting to floats
        if (r < 0.6f) return normalUFO;      // 60%
        else if (r < 0.9f) return droneUFO; // 30%
        else return bossUFO;               // 10%
    }

    // ---------------- EVENT CALLBACKS ---------------- //

    private void HandleUFODestroyed(int money)
    {
        moneyCount += money;
        ufoDown++;

        OnMoneyUpdated.Invoke(moneyCount);
        OnUFODownUpdated.Invoke(ufoDown);

        CheckCompletion();
    }

    private void HandleUFOFleed()
    {
        ufoFleed++;

        OnUFOFleedUpdated.Invoke(ufoFleed);

        CheckCompletion();
    }

    private void HandleCowLost()
    {
        cowsLost++;

        OnCowsLostUpdated.Invoke(cowsLost);
    }

    private void CheckCompletion()
    {
        activeUFOs--;

        if (activeUFOs <= 0 && spawnedUFOs >= totalUFOCount)
        {
            OnAllUFODone.Invoke();
        }
    }
}
