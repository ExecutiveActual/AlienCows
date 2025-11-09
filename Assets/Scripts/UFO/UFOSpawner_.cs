using System;
using System.Collections.Generic;
using UnityEngine;

public class UFOSpawner_ : MonoBehaviour
{
    [Serializable]
    public class WaveDefinition
    {
        [Tooltip("Type A (Circle/Spiral) count.")]
        public int countA = 0;

        [Tooltip("Type B (Parabola) count.")]
        public int countB = 0;

        [Tooltip("Type C (Sine) count.")]
        public int countC = 0;

        [Tooltip("Type D (Hyperbola) count.")]
        public int countD = 0;

        [Tooltip("Type E (Inverse X) count.")]
        public int countE = 0;

        [Tooltip("Optional per-wave spawn points. If empty, global Spawn Points will be used.")]
        public Transform[] specificSpawnPoints;

        [Tooltip("Seconds between individual spawns within this wave.")]
        public float spawnInterval = 0.25f;
    }

    [Header("References")]
    [Tooltip("World time reference.")]
    public WorldClock_ worldClock;

    [Tooltip("Prefab that has a UFO_ component.")]
    public GameObject ufoPrefab;

    [Tooltip("Default spawn points used when a Wave does not specify its own.")]
    public Transform[] spawnPoints; // up to 10

    [Header("Behavior")]
    [Tooltip("Remove all active UFOs instantly at daybreak.")]
    public bool despawnAtDay = true;

    [Tooltip("Maximum UFOs allowed simultaneously (0 = unlimited).")]
    public int maxConcurrentUFOs = 0;

    [Header("Waves (Inspector-Editable)")]
    [Tooltip("Define the composition of each wave here.")]
    public WaveDefinition[] waves = new WaveDefinition[3];

    // Runtime
    private readonly List<UFO_> activeUFOs = new List<UFO_>();
    private int currentWaveIndex = 0;
    private bool nightSpawnTriggered = false;
    private float spawnTimer = 0f;
    private Queue<UFO_.UFOType> spawnQueue;

    private void Reset()
    {
        // Default wave composition per your spec
        waves = new WaveDefinition[3];
        waves[0] = new WaveDefinition { countA = 3, countB = 1, countC = 0, countD = 0, countE = 0, spawnInterval = 0.25f };
        waves[1] = new WaveDefinition { countA = 3, countB = 2, countC = 0, countD = 1, countE = 2, spawnInterval = 0.25f };
        waves[2] = new WaveDefinition { countA = 2, countB = 2, countC = 2, countD = 2, countE = 2, spawnInterval = 0.25f };
    }

    private void Awake()
    {
        if (worldClock == null) worldClock = FindObjectOfType<WorldClock_>();
        if (waves == null || waves.Length == 0) Reset();
    }

    private void Update()
    {
        if (worldClock == null) { Debug.Log("WorldClock_ not found for UFOSpawner_."); return; }

        if (worldClock.isDay)
        {
            nightSpawnTriggered = false;
            spawnQueue = null;
            spawnTimer = 0f;

            if (despawnAtDay && activeUFOs.Count > 0)
            {
                // Destroy all active UFOs at daybreak
                for (int i = activeUFOs.Count - 1; i >= 0; i--)
                {
                    if (activeUFOs[i] != null)
                        Destroy(activeUFOs[i].gameObject);
                }
                activeUFOs.Clear();
            }

            return;
        }

        // Night
        if (!nightSpawnTriggered)
        {
            BeginWave(currentWaveIndex);
            nightSpawnTriggered = true;
        }

        // Spawn queued UFOs with interval pacing
        if (spawnQueue != null && spawnQueue.Count > 0)
        {
            spawnTimer -= Time.deltaTime;
            if (spawnTimer <= 0f)
            {
                if (maxConcurrentUFOs <= 0 || activeUFOs.Count < maxConcurrentUFOs)
                {
                    SpawnNextFromQueue();
                    var wd = GetWave(currentWaveIndex);
                    spawnTimer = Mathf.Max(0f, (wd != null ? wd.spawnInterval : 0.25f));
                }
            }
        }

        // When wave is cleared (all spawned and none active), prepare next night’s wave
        if ((spawnQueue == null || spawnQueue.Count == 0) && activeUFOs.Count == 0 && nightSpawnTriggered)
        {
            // Wave ended. Advance to next wave for the next night.
            currentWaveIndex = Mathf.Clamp(currentWaveIndex + 1, 0, waves.Length - 1);
            nightSpawnTriggered = false;
        }
    }

    private WaveDefinition GetWave(int index)
    {
        if (waves == null || waves.Length == 0) return null;
        index = Mathf.Clamp(index, 0, waves.Length - 1);
        return waves[index];
    }

    private void BeginWave(int index)
    {
        WaveDefinition wd = GetWave(index);
        if (wd == null || ufoPrefab == null)
            return;

        spawnQueue = new Queue<UFO_.UFOType>();

        Enqueue(spawnQueue, UFO_.UFOType.A_CircleSpiral, wd.countA);
        Enqueue(spawnQueue, UFO_.UFOType.B_Parabola,     wd.countB);
        Enqueue(spawnQueue, UFO_.UFOType.C_Sine,         wd.countC);
        Enqueue(spawnQueue, UFO_.UFOType.D_Hyperbola,    wd.countD);
        Enqueue(spawnQueue, UFO_.UFOType.E_InverseX,     wd.countE);

        // Randomize order a bit for variety
        var tmp = new List<UFO_.UFOType>(spawnQueue);
        Shuffle(tmp);
        spawnQueue.Clear();
        foreach (var t in tmp) spawnQueue.Enqueue(t);

        spawnTimer = 0f; // spawn immediately
    }

    private void SpawnNextFromQueue()
    {
        if (spawnQueue == null || spawnQueue.Count == 0) return;

        UFO_.UFOType nextType = spawnQueue.Dequeue();

        Transform sp = PickSpawnPointForCurrentWave();
        if (sp == null)
        {
            Debug.Log("No spawn points assigned for UFOSpawner_.");
            return;
        }

        GameObject go = Instantiate(ufoPrefab, new Vector3(sp.position.x, 0f, sp.position.z), sp.rotation);
        var ufo = go.GetComponent<UFO_>();
        if (ufo == null)
        {
            Debug.Log("Spawned prefab does not have UFO_ component.");
            Destroy(go);
            return;
        }

        // Configure the spawned UFO
        ufo.type = nextType;
        ufo.spawnerRef = this;
        if (ufo.worldClock == null) ufo.worldClock = worldClock;

        // Ensure correct height immediately
        var p = ufo.transform.position;
        ufo.transform.position = new Vector3(p.x, ufo.flyHeight, p.z);

        activeUFOs.Add(ufo);
    }

    private Transform PickSpawnPointForCurrentWave()
    {
        WaveDefinition wd = GetWave(currentWaveIndex);
        Transform[] set = (wd != null && wd.specificSpawnPoints != null && wd.specificSpawnPoints.Length > 0)
            ? wd.specificSpawnPoints
            : spawnPoints;

        if (set == null || set.Length == 0) return null;

        int idx = UnityEngine.Random.Range(0, set.Length);
        return set[idx];
    }

    public void NotifyUfoDestroyed(UFO_ ufo)
    {
        if (ufo == null) return;
        int i = activeUFOs.IndexOf(ufo);
        if (i >= 0) activeUFOs.RemoveAt(i);
    }

    private static void Enqueue(Queue<UFO_.UFOType> q, UFO_.UFOType t, int count)
    {
        if (count <= 0) return;
        for (int i = 0; i < count; i++) q.Enqueue(t);
    }

    private static void Shuffle<T>(IList<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            T tmp = list[i];
            list[i] = list[j];
            list[j] = tmp;
        }
    }
}
