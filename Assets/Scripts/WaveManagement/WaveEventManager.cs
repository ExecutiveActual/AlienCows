using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class WaveEventManager : MonoBehaviour
{

    public static WaveEventManager Instance { get; private set; }


    private List<WaveEntity> waveEntities = new List<WaveEntity>();
    
    private List<WaveSpawner> waveSpawners = new List<WaveSpawner>();


    /// <summary>
    /// Triggered when all waveEntities are destroyed and all waveSpawners are done.
    /// </summary>
    public UnityEvent OnWaveEnd;



    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
    }


    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }



    public void WaveSpawner_DoneSpawning(WaveSpawner waveSpawner)
    {
        //  !REWORK!
        //Unregister_WaveSpawner(waveSpawner);
        Debug.LogWarning("<color=white>Waiting for WaveSpawn Rework!</color>");
        //  !REWORK!



        // If no more spawners and no more entities, wave is over
        if (waveSpawners.Count == 0 && waveEntities.Count == 0)
        {
            OnWaveEnd?.Invoke();
        }
    }




    #region Registry Methods

    // All WaveSpawners must register on Start
    public void Register_WaveSpawner(WaveSpawner waveSpawner)
    {
        if (!waveSpawners.Contains(waveSpawner))
        {
            waveSpawners.Add(waveSpawner);
        }
    }

    // All WaveSpawners must unregister when done spawning
    public void Unregister_WaveSpawner(WaveSpawner waveSpawner)
    {
        if (waveSpawners.Contains(waveSpawner))
        {
            waveSpawners.Remove(waveSpawner);
        }
    }

    // Check if a WaveSpawner is registered
    public bool CheckRegistryFor_WaveSpawner(WaveSpawner waveSpawner)
    {
        return waveSpawners.Contains(waveSpawner);
    }

    // DANGEROUS! USE WITH CAUTION!
    private void ClearRegistry_WaveSpawner()
    {
        waveSpawners.Clear();
    }


    // All entities spawned in wave must register on Start
    public void Register_WaveEntity(WaveEntity entity)
    {
        if (!waveEntities.Contains(entity))
        {
            waveEntities.Add(entity);
        }
    }

    // All entities spawned in wave must unregister OnDestroy
    public void Unregister_WaveEntity(WaveEntity entity)
    {
        if (waveEntities.Contains(entity))
        {
            waveEntities.Remove(entity);
        }
    }

    // Check if a WaveEntity is registered
    public bool CheckRegistryFor_WaveEntity(WaveEntity entity)
    {
        return waveEntities.Contains(entity);
    }

    // DANGEROUS! USE WITH CAUTION!
    private void ClearRegistry_WaveEntity()
    {
        waveEntities.Clear();
    }

    #endregion

}
