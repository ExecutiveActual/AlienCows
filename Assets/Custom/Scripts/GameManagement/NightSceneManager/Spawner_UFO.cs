using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner_UFO : MonoBehaviour
{

    GameManager_NightCounter nightCounter;



    [SerializeField] private SO_WaveTable waveTable;

    [SerializeField] private GameObject ufoPrefab;

    [SerializeField] private Transform SpawnPointParent;

    private Transform[] ufoSpawnPoints;

    [SerializeField] private float spawnRandomSpread = 5f;

    [SerializeField] private float startSpawnDelay = 3f;

    [SerializeField] private float spawnDelay = 2f;


    private int spawnAmount = 0;

    private List<UFO_WaveRegistry> waveRegistry = new List<UFO_WaveRegistry>();

    Coroutine spawnCoroutine;

    private bool isDoneSpawning = false; 





    private void Awake()
    {
        ufoSpawnPoints = SpawnPointParent.GetComponentsInChildren<Transform>();
    }


    private void Start()
    {
        
        nightCounter = GameManager_Singleton.Instance.GetComponent<GameManager_NightCounter>();

        Debug.Log($"Night #{nightCounter.GetNightNumberCurrent()}");

        spawnAmount = waveTable.amountPerNight[nightCounter.GetNightNumberCurrent() - 1];


        if (spawnCoroutine == null)
        {
            isDoneSpawning = false;

            spawnCoroutine = StartCoroutine(spawnRoutine());
        }

    }



    private IEnumerator spawnRoutine()
    {
        Debug.Log("UFO   Waiting Start");

        yield return new WaitForSeconds(startSpawnDelay);


        for (int i = 0; i < spawnAmount; i++)
        {

            SpawnUFO();

            Debug.Log("UFO   Spawned");


            yield return new WaitForSeconds(spawnDelay);

        }

        isDoneSpawning = true;

        Debug.Log("UFO   DOne");

    }


    private void SpawnUFO()
    {
        Transform spawnPoint = GetRandomSpawnPoint();
        GameObject clone = Instantiate(ufoPrefab, GetRandomPos(spawnPoint.position, spawnRandomSpread), Quaternion.identity);
        clone.GetComponent<UFO_WaveRegistry>().Register(this);
    }


    private Transform GetRandomSpawnPoint()
    {

        Debug.Log($"UFO   length: {ufoSpawnPoints.Length}");
        List<Transform> possibleSpawnPoints = new List<Transform>(ufoSpawnPoints);
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


    public void RegisterUFO(UFO_WaveRegistry newUFO)
    {
        waveRegistry.Add(newUFO);
    }

    public void UnregisterUFO(UFO_WaveRegistry oldUFO)
    {
        waveRegistry.Remove(oldUFO);

        Debug.Log("UFO   Unregistered");

        if (isDoneSpawning && waveRegistry.Count == 0)
        {
            Debug.Log("UFO   All Gone");
            NightSceneManager nightSceneManager = GetComponentInParent<NightSceneManager>();
            nightSceneManager.OnAllWavesComplete();
        }
    }


}