using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UFOSpawner_ : MonoBehaviour
{
    [Header("Spawner Settings")]
    public WorldClock_ worldClock;
    [Tooltip("Prefab of the UFO to spawn.")]
    public GameObject ufoPrefab;
    [Tooltip("Possible spawn points for UFOs.")]
    public Transform[] spawnPoints;
    [Tooltip("How many UFOs to spawn per night wave.")]
    public int ufoCountPerWave = 5;

    [Header("Timing Settings")]
    [Tooltip("Delay after night starts before spawning UFOs.")]
    public float nightStartDelay = 2f;

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

        // Spawn a wave when night begins
        if (!waveSpawned && !worldClock.isDay)
        {
            waveSpawned = true;
            StartCoroutine(SpawnWaveAfterDelay(nightStartDelay));
        }

        // Reset once day returns
        if (worldClock.isDay && waveSpawned)
        {
            waveSpawned = false;
        }
    }

    private IEnumerator SpawnWaveAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        SpawnWave();
    }

    private void SpawnWave()
    {
        if (!ufoPrefab || spawnPoints.Length == 0)
        {
            Debug.LogWarning("[UFOSpawner_] Missing prefab or spawn points!");
            return;
        }

        for (int i = 0; i < ufoCountPerWave; i++)
        {
            Transform spawnAt = spawnPoints[Random.Range(0, spawnPoints.Length)];
            Vector3 spawnPos = new Vector3(spawnAt.position.x, 25f, spawnAt.position.z);

            GameObject ufo = Instantiate(ufoPrefab, spawnPos, Quaternion.identity);
            UFO_ ufoScript = ufo.GetComponent<UFO_>();

            if (ufoScript)
            {
                ufoScript.spawnerRef = this;
                ufoScript.worldClock = worldClock;
            }

            activeUFOs.Add(ufo);
        }

        Debug.Log($"[UFOSpawner_] Wave spawned: {activeUFOs.Count} UFOs.");
    }

    public void NotifyUfoDestroyed(UFO_ ufo)
    {
        if (ufo)
            activeUFOs.Remove(ufo.gameObject);
    }
}
