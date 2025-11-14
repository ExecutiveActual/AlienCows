using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Professional UFO spawner that integrates with WorldClock_.
/// - Waves are pre-planned in the inspector (one WaveInfo per night).
/// - Each spawned unit uses a unique spawn point for that wave.
/// - Uses UnityEvents for all designer callbacks.
/// - Tracks runtime counts (spawned / alive / destroyed / fled).
/// 
/// Usage:
/// 1) Assign WorldClock_, spawn points, and prefabs in inspector.
/// 2) Configure WaveInfo list (index 0 -> Night 1, index 1 -> Night 2, ...).
/// 3) Hook UnityEvents to UI/game systems as needed.
/// 4) Make enemies call ReportEnemyDestroyed/ReportEnemyFled to update counts.
/// </summary>
public class UFOSpawner : MonoBehaviour
{
    //===========================//
    //      SIMPLE ENUMS         //
    //===========================//
    public enum EnemyType { UFO, Drone, Boss }

    //===========================//
    //     INSPECTOR DATA        //
    //===========================//

    [Header("References")]
    [Tooltip("Reference to the WorldClock_ in the scene. Used to check night-state and night number.")]
    public WorldClock_ worldClock;

    [Tooltip("Spawn points assigned in the scene. Each wave will pick unique points from this list.")]
    public List<Transform> spawnPoints = new List<Transform>();

    [Header("Enemy Prefabs")]
    [Tooltip("UFO prefab to spawn.")]
    public GameObject ufoPrefab;

    [Tooltip("Drone prefab to spawn.")]
    public GameObject dronePrefab;

    [Tooltip("Boss prefab to spawn.")]
    public GameObject bossPrefab;

    [Header("Wave Configuration (Inspector)")]
    [Tooltip("List of pre-planned waves. waves[0] = Night 1, waves[1] = Night 2, ...")]
    public List<WaveInfo> waves = new List<WaveInfo>();

    [Header("Spawn Timing")]
    [Tooltip("Delay between spawning each unit in the wave (seconds).")]
    [Range(0f, 5f)]
    public float timeBetweenSpawns = 0.25f;

    [Tooltip("Should the spawner automatically start the wave when WorldClock_ enters night?")]
    public bool autoStartOnNight = true;

    [Header("Runtime Debug")]
    [Tooltip("Enable verbose debug logs for the spawner.")]
    public bool showDebugLogs = false;


    //===========================//
    //      UNITY EVENTS         //
    //===========================//

    [Header("UnityEvents (Designer Friendly)")]
    [Tooltip("Invoked when a wave starts.")]
    public UnityEvent onWaveStarted;

    [Tooltip("Invoked when a wave ends (all spawned enemies have been cleared or fled).")]
    public UnityEvent onWaveEnded;

    [Tooltip("Invoked when any enemy is spawned. Passes the spawned GameObject.")]
    public UnityEvent<GameObject> onEnemySpawned;

    [Tooltip("Invoked when all enemies for the wave are gone (destroyed/fled).")]
    public UnityEvent onAllEnemiesEliminated;

    [Tooltip("Invoked if/when boss spawns for the current wave.")]
    public UnityEvent onBossAppeared;

    [Tooltip("Invoked any time counts change. Int parameter is total alive across all types.")]
    public UnityEvent<int> onAliveCountChanged;


    //===========================//
    //     RUNTIME TRACKING      //
    //===========================//

    // Public read-only properties for other systems to query.
    public int TotalSpawnedThisWave { get; private set; }
    public int TotalAliveThisWave { get; private set; }
    public int TotalDestroyedThisWave { get; private set; }
    public int TotalFledThisWave { get; private set; }

    // Per-type breakdowns
    public int SpawnedUFOs { get; private set; }
    public int SpawnedDrones { get; private set; }
    public int SpawnedBosses { get; private set; }

    public int AliveUFOs { get; private set; }
    public int AliveDrones { get; private set; }
    public int AliveBosses { get; private set; }

    public int DestroyedUFOs { get; private set; }
    public int DestroyedDrones { get; private set; }

    public int FledUFOs { get; private set; }
    public int FledDrones { get; private set; }

    // Internal state
    private bool waveActive = false;
    private int currentNightIndex = -1; // zero-based for waves list
    private Coroutine spawnCoroutine = null;

    // Tracks which spawn indices have been used this wave (unique spawn per point)
    private HashSet<int> usedSpawnIndexes = new HashSet<int>();


    //===========================//
    //      SERIALIZABLE TYPES   //
    //===========================//

    [System.Serializable]
    public class WaveInfo
    {
        [Tooltip("Number of UFOs to spawn for this wave.")]
        public int ufoCount = 0;

        [Tooltip("Number of Drones to spawn for this wave.")]
        public int droneCount = 0;

        [Tooltip("Number of Bosses to spawn for this wave (usually 0 or 1).")]
        public int bossCount = 0;

        [Tooltip("Optional name for the wave / night to display in UI.")]
        public string label = "";
    }


    //===========================//
    //         UNITY FLOW        //
    //===========================//

    private void OnEnable()
    {
        // Subscribe to world clock changes if autoStartOnNight is enabled.
        if (worldClock != null && autoStartOnNight)
        {
            // We'll poll WorldClock's session at Start; no direct event published by WorldClock_
            // but we can start a coroutine to watch transitions. Simpler: start a watcher.
            StartCoroutine(WorldClockWatcher());
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private void Start()
    {
        ValidateSetup();
    }


    //===========================//
    //      PUBLIC API CALLS     //
    //===========================//

    /// <summary>
    /// Forces the spawner to attempt to start the wave for the currently active night in WorldClock_.
    /// Use this for manual triggers (e.g., button press in editor).
    /// </summary>
    [ContextMenu("Start Wave For Current Night")]
    public void StartWaveForCurrentNight()
    {
        if (worldClock == null)
        {
            Debug.LogWarning("[UFOSpawner] Cannot start wave: WorldClock_ not assigned.");
            return;
        }

        if (!worldClock.IsNight())
        {
            if (showDebugLogs) Debug.Log("[UFOSpawner] WorldClock_ reports it's not night. Wave start skipped.");
            return;
        }

        int nightNumber = worldClock.CurrentNight; // 1-based in WorldClock_
        StartWaveByNightNumber(nightNumber);
    }

    /// <summary>
    /// Start the wave corresponding to the given night number (1-based).
    /// This method checks bounds and resets runtime counters.
    /// </summary>
    public void StartWaveByNightNumber(int nightNumber)
    {
        int waveIndex = nightNumber - 1;
        if (waveIndex < 0 || waveIndex >= waves.Count)
        {
            Debug.LogWarning($"[UFOSpawner] No WaveInfo found for Night {nightNumber} (index {waveIndex}).");
            return;
        }

        if (waveActive)
        {
            if (showDebugLogs) Debug.Log("[UFOSpawner] Wave already active. StartWave request ignored.");
            return;
        }

        // Prepare runtime state
        ResetRuntimeCounters();
        usedSpawnIndexes.Clear();
        currentNightIndex = waveIndex;

        WaveInfo w = waves[waveIndex];
        TotalSpawnedThisWave = w.ufoCount + w.droneCount + w.bossCount;

        if (showDebugLogs) Debug.Log($"[UFOSpawner] Starting wave {waveIndex + 1}. Total to spawn: {TotalSpawnedThisWave}");

        // Start spawn coroutine
        waveActive = true;
        spawnCoroutine = StartCoroutine(SpawnWaveRoutine(w));
        onWaveStarted?.Invoke();
    }

    /// <summary>
    /// Public method for enemies to report that they got destroyed.
    /// Enemies should call this on death so the spawner can track counts.
    /// </summary>
    public void ReportEnemyDestroyed(EnemyType type)
    {
        switch (type)
        {
            case EnemyType.UFO:
                DestroyedUFOs++;
                AliveUFOs = Mathf.Max(0, AliveUFOs - 1);
                DestroyedUFOs = Mathf.Max(0, DestroyedUFOs);
                DestroyedUFOs = DestroyedUFOs;
                DestroyedUFOs = DestroyedUFOs; // no-op lines removed by compiler but kept here for pattern visibility
                DestroyedUFOs = DestroyedUFOs;
                DestroyedUFOs = DestroyedUFOs;
                DestroyedUFOs = DestroyedUFOs;
                break;
            case EnemyType.Drone:
                DestroyedDrones++;
                AliveDrones = Mathf.Max(0, AliveDrones - 1);
                break;
            case EnemyType.Boss:
                AliveBosses = Mathf.Max(0, AliveBosses - 1);
                break;
        }

        UpdateTotalsAndNotify();
        TryEndWaveIfCleared();
    }

    /// <summary>
    /// Public method for enemies to report that they fled the scene (or were otherwise lost).
    /// Enemies should call this when they leave without being destroyed.
    /// </summary>
    public void ReportEnemyFled(EnemyType type)
    {
        switch (type)
        {
            case EnemyType.UFO:
                FledUFOs++;
                AliveUFOs = Mathf.Max(0, AliveUFOs - 1);
                break;
            case EnemyType.Drone:
                FledDrones++;
                AliveDrones = Mathf.Max(0, AliveDrones - 1);
                break;
            case EnemyType.Boss:
                AliveBosses = Mathf.Max(0, AliveBosses - 1);
                break;
        }

        UpdateTotalsAndNotify();
        TryEndWaveIfCleared();
    }

    /// <summary>
    /// Manually stop the active wave (stops spawning, doesn't destroy existing enemies).
    /// Useful for debugging or force-cleanups.
    /// </summary>
    public void StopWave()
    {
        if (!waveActive) return;

        if (spawnCoroutine != null)
            StopCoroutine(spawnCoroutine);

        waveActive = false;
        spawnCoroutine = null;
        if (showDebugLogs) Debug.Log("[UFOSpawner] Wave forcibly stopped.");
        onWaveEnded?.Invoke();
    }


    //===========================//
    //      INTERNAL LOGIC       //
    //===========================//

    private IEnumerator WorldClockWatcher()
    {
        // Watch for transition to Night in WorldClock_ and start waves automatically.
        // Polled watcher is intentionally lightweight to avoid coupling.
        int lastNight = -1;
        bool lastWasNight = false;

        while (true)
        {
            if (worldClock != null)
            {
                bool isNight = worldClock.IsNight();
                int nightNum = worldClock.CurrentNight;

                if (isNight && !lastWasNight)
                {
                    // Night just started
                    if (showDebugLogs) Debug.Log($"[UFOSpawner] Detected WorldClock night start: Night {nightNum}");
                    StartWaveByNightNumber(nightNum);
                }

                lastWasNight = isNight;
                lastNight = nightNum;
            }

            yield return new WaitForSeconds(0.1f); // light polling interval
        }
    }

    private IEnumerator SpawnWaveRoutine(WaveInfo wave)
    {
        // Prepare a list of spawn tasks preserving enemy-type grouping (optional).
        List<EnemyType> spawnPlan = new List<EnemyType>();

        for (int i = 0; i < wave.ufoCount; i++) spawnPlan.Add(EnemyType.UFO);
        for (int i = 0; i < wave.droneCount; i++) spawnPlan.Add(EnemyType.Drone);
        for (int i = 0; i < wave.bossCount; i++) spawnPlan.Add(EnemyType.Boss);

        // Defensive: Do not attempt more spawns than available spawn points.
        if (spawnPlan.Count > spawnPoints.Count)
        {
            Debug.LogWarning($"[UFOSpawner] Wave requires {spawnPlan.Count} spawns but only {spawnPoints.Count} spawn points exist. Extra spawns will be skipped.");
        }

        // Randomize order across spawnPlan if you want variety of spawn positions (keeps counts fixed).
        // NOTE: Wave composition (counts) remains fixed; only ordering of spawn positions is variable.
        ShuffleList(spawnPlan);

        for (int i = 0; i < spawnPlan.Count; i++)
        {
            if (!waveActive) yield break; // wave was cancelled externally

            if (usedSpawnIndexes.Count >= spawnPoints.Count)
            {
                // No more unique spawn points available; stop spawning further units.
                if (showDebugLogs) Debug.Log("[UFOSpawner] All spawn points occupied — stopping further spawns for this wave.");
                break;
            }

            // Pick a unique spawn index
            int chosenIndex = PickUniqueSpawnIndex();
            if (chosenIndex < 0 || chosenIndex >= spawnPoints.Count)
            {
                if (showDebugLogs) Debug.LogWarning("[UFOSpawner] Invalid spawn index chosen. Skipping this spawn.");
                continue;
            }

            Transform spawnPoint = spawnPoints[chosenIndex];
            EnemyType typeToSpawn = spawnPlan[i];

            GameObject prefab = GetPrefabForType(typeToSpawn);
            if (prefab == null)
            {
                if (showDebugLogs) Debug.LogWarning($"[UFOSpawner] Prefab missing for {typeToSpawn}. Skipping spawn.");
                continue;
            }

            // Instantiate
            GameObject spawned = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation, null);

            // Increment runtime counters
            RegisterSpawned(typeToSpawn);

            // Optionally tag/spawn-component to allow enemy to know the spawner reference (if you want)
            var marker = spawned.AddComponent<SpawnedByUFOSpawner>();
            marker.spawner = this;
            marker.type = typeToSpawn;

            if (typeToSpawn == EnemyType.Boss)
                onBossAppeared?.Invoke();

            onEnemySpawned?.Invoke(spawned);

            if (showDebugLogs) Debug.Log($"[UFOSpawner] Spawned {typeToSpawn} at spawn point {chosenIndex}.");

            // Wait before next spawn
            yield return new WaitForSeconds(timeBetweenSpawns);
        }

        // All spawns scheduled — now just wait for the wave to finish naturally
        if (showDebugLogs) Debug.Log("[UFOSpawner] All scheduled spawns spawned. Waiting for elimination/flee.");
        // Exit coroutine (waveActive remains true until TryEndWaveIfCleared observes all cleared)
        spawnCoroutine = null;
        yield break;
    }

    private void RegisterSpawned(EnemyType type)
    {
        TotalSpawnedThisWave++;
        switch (type)
        {
            case EnemyType.UFO:
                SpawnedUFOs++;
                AliveUFOs++;
                break;
            case EnemyType.Drone:
                SpawnedDrones++;
                AliveDrones++;
                break;
            case EnemyType.Boss:
                SpawnedBosses++;
                AliveBosses++;
                break;
        }

        UpdateTotalsAndNotify();
    }

    private void UpdateTotalsAndNotify()
    {
        TotalAliveThisWave = AliveUFOs + AliveDrones + AliveBosses;
        TotalDestroyedThisWave = DestroyedUFOs + DestroyedDrones;
        TotalFledThisWave = FledUFOs + FledDrones;

        onAliveCountChanged?.Invoke(TotalAliveThisWave);
    }

    private void TryEndWaveIfCleared()
    {
        // End wave when all spawned units are no longer alive (either destroyed or fled).
        int expectedTotalSpawned = 0;
        if (currentNightIndex >= 0 && currentNightIndex < waves.Count)
        {
            var w = waves[currentNightIndex];
            expectedTotalSpawned = w.ufoCount + w.droneCount + w.bossCount;
        }

        int totalCleared = TotalDestroyedThisWave + TotalFledThisWave;
        bool noLongerAlive = (TotalAliveThisWave <= 0);
        bool allSpawnedAccounted = (totalCleared >= expectedTotalSpawned && TotalSpawnedThisWave >= expectedTotalSpawned);

        if (noLongerAlive && allSpawnedAccounted)
        {
            // Wave finished
            waveActive = false;
            if (showDebugLogs) Debug.Log("[UFOSpawner] Wave cleared. Invoking wave-end events.");
            onAllEnemiesEliminated?.Invoke();
            onWaveEnded?.Invoke();
        }
    }

    private GameObject GetPrefabForType(EnemyType t)
    {
        switch (t)
        {
            case EnemyType.UFO: return ufoPrefab;
            case EnemyType.Drone: return dronePrefab;
            case EnemyType.Boss: return bossPrefab;
        }
        return null;
    }

    private int PickUniqueSpawnIndex()
    {
        if (spawnPoints == null || spawnPoints.Count == 0) return -1;

        // Attempt random choice up to N times to find an unused index
        int tries = 0;
        int maxTries = spawnPoints.Count * 2;
        while (tries < maxTries)
        {
            int idx = Random.Range(0, spawnPoints.Count);
            if (!usedSpawnIndexes.Contains(idx))
            {
                usedSpawnIndexes.Add(idx);
                return idx;
            }
            tries++;
        }

        // As a fallback: linear scan for any unused index
        for (int i = 0; i < spawnPoints.Count; i++)
        {
            if (!usedSpawnIndexes.Contains(i))
            {
                usedSpawnIndexes.Add(i);
                return i;
            }
        }

        // All used
        return -1;
    }

    // Utility: Fisher-Yates shuffle for List<T>
    private void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int j = Random.Range(i, list.Count);
            T tmp = list[i];
            list[i] = list[j];
            list[j] = tmp;
        }
    }

    private void ResetRuntimeCounters()
    {
        TotalSpawnedThisWave = 0;
        TotalAliveThisWave = 0;
        TotalDestroyedThisWave = 0;
        TotalFledThisWave = 0;

        SpawnedUFOs = SpawnedDrones = SpawnedBosses = 0;
        AliveUFOs = AliveDrones = AliveBosses = 0;
        DestroyedUFOs = DestroyedDrones = 0;
        FledUFOs = FledDrones = 0;
    }

    private void ValidateSetup()
    {
        if (worldClock == null)
        {
            Debug.LogWarning("[UFOSpawner] WorldClock_ not assigned. If autoStartOnNight is enabled, assign WorldClock_ in inspector.");
        }

        if (spawnPoints == null || spawnPoints.Count == 0)
        {
            Debug.LogWarning("[UFOSpawner] No spawn points assigned.");
        }

        if (ufoPrefab == null && dronePrefab == null && bossPrefab == null)
        {
            Debug.LogWarning("[UFOSpawner] No enemy prefabs assigned.");
        }
    }

    //===========================//
    //   HELPER: debug display   //
    //===========================//
#if UNITY_EDITOR
    private void OnValidate()
    {
        // Clamp negative values and ensure default wave list length is not less than WorldClock totalNights (optional)
        timeBetweenSpawns = Mathf.Max(0f, timeBetweenSpawns);
    }
#endif

    //===========================//
    //   HELPER: spawn marker    //
    //===========================//
    /// <summary>
    /// Minimal helper component attached to spawned prefabs so they can report back to the spawner.
    /// Enemies may optionally call spawner.ReportEnemyDestroyed / ReportEnemyFled from their death code.
    /// If you'd like automatic reporting, you can expand this small component to forward events.
    /// </summary>
    private class SpawnedByUFOSpawner : MonoBehaviour
    {
        public UFOSpawner spawner;
        public EnemyType type;
    }
}
