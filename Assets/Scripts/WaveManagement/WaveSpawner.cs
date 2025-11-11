using UnityEngine;

public class WaveSpawner : MonoBehaviour
{

    WaveEventManager waveEventManager;

    protected virtual void Start()
    {
        waveEventManager = WaveEventManager.Instance;

        if (waveEventManager != null)
        {
            waveEventManager.Register_WaveSpawner(this);
        }

        if (waveEventManager.CheckRegistryFor_WaveSpawner(this) == false)
        {
            Debug.LogError($"WaveSpawner failed to register with WaveEventManager: {this.name}. DEACTIVATING!");

            //  !REWORK!
            //gameObject.SetActive(false);
            Debug.LogWarning("<color=white>Waiting for WaveSpawn Rework!</color>");
            //  !REWORK!

        }

    }



    protected void OnSpawnerDoneSpawning()
    {
        if (waveEventManager != null)
        {
            waveEventManager.WaveSpawner_DoneSpawning(this);
        }
    }


    //  !REWORK!
    protected void OnAllWaveEntitiesDestroyed()
    {
        Debug.LogWarning("<color=white>Waiting for WaveSpawn Rework!</color>");
    }
    //  !REWORK!


}
