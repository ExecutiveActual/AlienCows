using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UFOSpawner_ : MonoBehaviour
{
    [Header("Spawner Settings")]
    public WorldClock_ worldClock;
    public GameObject ufoPrefab;
    public Transform[] spawnPoints;
    public int ufoCountPerWave = 5;
    public float nightStartDelay = 2f;

    public event Action OnUfoSpawned;
    public event Action OnAllUfosDestroyed;

    private List<GameObject> activeUFOs = new List<GameObject>();
    private bool waveSpawned;

    void Start()
    {
        if (!worldClock)
            worldClock = FindObjectOfType<WorldClock_>();
    }

    void Update()
    {
        if (!worldClock) return;

        if (!waveSpawned && !worldClock.isDay)
        {
            waveSpawned = true;
            StartCoroutine(SpawnWaveAfterDelay(nightStartDelay));
        }

        if (worldClock.isDay && waveSpawned)
            waveSpawned = false;
    }

    private IEnumerator SpawnWaveAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SpawnWave();
    }

    private void SpawnWave()
    {
        if (!ufoPrefab || spawnPoints.Length == 0) return;

        for (int i = 0; i < ufoCountPerWave; i++)
        {
            Transform spawnAt = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)];
            Vector3 pos = new Vector3(spawnAt.position.x, 25f, spawnAt.position.z);

            GameObject ufo = Instantiate(ufoPrefab, pos, Quaternion.identity);
            UFO_ script = ufo.GetComponent<UFO_>();
            if (script)
            {
                script.spawnerRef = this;
                script.worldClock = worldClock;
            }
            activeUFOs.Add(ufo);
            OnUfoSpawned?.Invoke(); // notify AudioManager
        }

        Debug.Log($"[UFOSpawner_] Wave started → {activeUFOs.Count} UFOs spawned.");
    }

    public void NotifyUfoDestroyed(UFO_ ufo)
    {
        if (ufo)
            activeUFOs.Remove(ufo.gameObject);

        if (activeUFOs.Count == 0)
            OnAllUfosDestroyed?.Invoke(); // notify AudioManager
    }
}
