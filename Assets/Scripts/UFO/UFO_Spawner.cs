using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

[System.Serializable]
public enum UFOType { Saucer, Drone, Boss }

[System.Serializable]
public class WaveEntry
{
    public int saucerCount = 0;
    public int droneCount = 0;
    public int bossCount = 0;
    public float timeBetweenSpawns = 0.5f;
    public float initialDelay = 0f;
}

[System.Serializable]
public class SpawnerResult
{
    public int ufosKilled = 0;
    public int ufosFled = 0;
    public int cowsLost = 0;
    public int moneyGained = 0;
}

public class UFO_Spawner : MonoBehaviour
{
    [Header("World Clock")]
    public WorldClock_ worldClock;

    [Header("Prefabs")]
    public GameObject saucerPrefab;
    public GameObject dronePrefab;
    public GameObject bossPrefab;

    [Header("Spawn Settings")]
    public Transform[] spawnPoints;
    public int maxActiveUFOs = 20;

    [Header("Waves (Night 1 = index 0)")]
    public List<WaveEntry> waves = new List<WaveEntry>(15);

    [Header("Economy")]
    public int moneyPerUFODestroyed = 50;

    [Header("Cleanup / Results")]
    public float cleanupDelayAfterNight = 2f;

    [Header("UI Manager")]
    public UFO_UI_Manager uiManager;

    [Header("Events")]
    public UnityEvent<int> OnWaveStart;
    public UnityEvent<int> OnWaveEnd;
    public UnityEvent OnAllUFOsCleared;

    // Internal
    private List<GameObject> activeUFOs = new List<GameObject>();
    private SpawnerResult result = new SpawnerResult();
    private bool isNightActive = false;
    private bool nightSpawnStarted = false;
    private Coroutine spawnRoutine;

    private WorldClock_.TimeSession lastSession = WorldClock_.TimeSession.Idle;


    // ---------------------- UNITY UPDATE ----------------------
    void Update()
    {
        if (!worldClock) return;

        // Detect session transitions
        var session = worldClock.currentSession;

        // NIGHT START
        if (session == WorldClock_.TimeSession.InGame_Night && lastSession != session)
        {
            StartNight(worldClock.CurrentNight);
        }

        // DAY START
        if (session == WorldClock_.TimeSession.InGame_Day && lastSession != session)
        {
            EndNight();
        }

        lastSession = session;
    }


    // ---------------------- NIGHT LOGIC ----------------------
    void StartNight(int nightNumber)
    {
        if (isNightActive) return;

        isNightActive = true;
        nightSpawnStarted = true;

        ResetResult();

        OnWaveStart?.Invoke(nightNumber);
        uiManager?.ShowWaveStart(nightNumber);

        spawnRoutine = StartCoroutine(RunWave(nightNumber));
    }

    void EndNight()
    {
        if (!isNightActive) return;

        isNightActive = false;
        nightSpawnStarted = false;

        StopAllCoroutines();
        CleanupRemainingUFOs();

        StartCoroutine(ShowFinalResultsAfterDelay());
    }


    // ---------------------- WAVE SYSTEM ----------------------
    IEnumerator RunWave(int nightNumber)
    {
        if (waves.Count == 0) yield break;

        WaveEntry wave = waves[Mathf.Clamp(nightNumber - 1, 0, waves.Count - 1)];

        yield return new WaitForSeconds(wave.initialDelay);

        // spawn helper
        IEnumerator SpawnType(GameObject prefab, int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (!isNightActive) yield break;

                if (activeUFOs.Count >= maxActiveUFOs)
                {
                    yield return new WaitUntil(() => activeUFOs.Count < maxActiveUFOs || !isNightActive);
                    if (!isNightActive) yield break;
                }

                SpawnUFO(prefab);
                yield return new WaitForSeconds(wave.timeBetweenSpawns);
            }
        }

        yield return StartCoroutine(SpawnType(saucerPrefab, wave.saucerCount));
        yield return StartCoroutine(SpawnType(dronePrefab, wave.droneCount));
        yield return StartCoroutine(SpawnType(bossPrefab, wave.bossCount));

        // Wait until cleared OR night ends
        yield return new WaitUntil(() => !isNightActive || activeUFOs.Count == 0);

        if (!isNightActive) yield break;

        StartCoroutine(ShowFinalResultsAfterDelay());
    }


    // ---------------------- SPAWNING ----------------------
    void SpawnUFO(GameObject prefab)
    {
        if (!prefab || spawnPoints.Length == 0) return;

        Transform sp = spawnPoints[Random.Range(0, spawnPoints.Length)];
        GameObject go = Instantiate(prefab, sp.position, Quaternion.identity);

        activeUFOs.Add(go);

        // Attach listeners
        UFO_Health health = go.GetComponent<UFO_Health>();
        if (health)
        {
            health.OnUFODestroyed.AddListener(OnUFODestroyed);
        }

        UFO_Controller ctrl = go.GetComponent<UFO_Controller>();
        if (ctrl)
        {
            ctrl.OnUFOFled.AddListener(OnUFOFled);
            ctrl.OnCowAbducted.AddListener(OnCowAbducted);
            ctrl.OnUFOFinished.AddListener(OnUFOFinished);
        }
    }


    // ---------------------- EVENT HANDLERS ----------------------
    void OnUFODestroyed(UFO_Health h)
    {
        result.ufosKilled++;
        result.moneyGained += moneyPerUFODestroyed;

        uiManager?.UpdateUFODownText(result.ufosKilled);
        uiManager?.UpdateMoneyText(result.moneyGained);

        RemoveUFO(h.gameObject);
    }

    void OnUFOFled(UFO_Controller u)
    {
        result.ufosFled++;
        uiManager?.UpdateUFOFledText(result.ufosFled);
        RemoveUFO(u.gameObject);
    }

    void OnCowAbducted(UFO_Controller u)
    {
        result.cowsLost++;
        uiManager?.UpdateCowsLostText(result.cowsLost);
        uiManager?.ShowToast("Cow Abducted!", 2f);
    }

    void OnUFOFinished(UFO_Controller u)
    {
        RemoveUFO(u.gameObject);
    }


    // ---------------------- CLEANUP ----------------------
    void RemoveUFO(GameObject go)
    {
        if (activeUFOs.Contains(go))
            activeUFOs.Remove(go);

        if (!isNightActive && activeUFOs.Count == 0)
        {
            StartCoroutine(ShowFinalResultsAfterDelay());
        }
    }

    void CleanupRemainingUFOs()
    {
        foreach (var u in new List<GameObject>(activeUFOs))
        {
            if (!u) continue;

            var ctrl = u.GetComponent<UFO_Controller>();
            if (ctrl)
            {
                ctrl.ForceFleeToSpawnAndSelfDestruct();
            }
            else
            {
                Destroy(u);
            }
        }

        activeUFOs.Clear();
    }


    IEnumerator ShowFinalResultsAfterDelay()
    {
        yield return new WaitForSeconds(cleanupDelayAfterNight);

        OnWaveEnd?.Invoke(worldClock.CurrentNight);
        uiManager?.ShowResultPanel(result);
        OnAllUFOsCleared?.Invoke();

        ResetResult();
    }


    void ResetResult()
    {
        result.ufosKilled = 0;
        result.ufosFled = 0;
        result.cowsLost = 0;
        result.moneyGained = 0;
    }
}
