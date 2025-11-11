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
        Debug.Log($"<color=green>WaveSpawner Done Spawning:</color> {this.name} 1");
        if (waveEventManager != null)
        {
            waveEventManager.WaveSpawner_DoneSpawning(this);

            Debug.Log($"<color=green>WaveSpawner Done Spawning:</color> {this.name} 2");
        }
    }


    //  !REWORK!
    protected void OnAllWaveEntitiesDestroyed()
    {
        OnSpawnerDoneSpawning();

        Debug.LogWarning("<color=white>Waiting for WaveSpawn Rework!</color>");
    }
    //  !REWORK!


}
