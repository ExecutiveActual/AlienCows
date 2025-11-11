using UnityEngine;

public class WaveEntity : MonoBehaviour
{

    protected WaveEventManager waveEventManager;

    protected virtual void Start()
    {
        waveEventManager = WaveEventManager.Instance;

        if (waveEventManager != null)
        {
            waveEventManager.Register_WaveEntity(this);
        }

        if (waveEventManager.CheckRegistryFor_WaveEntity(this) == false)
        {
            Debug.LogError($"WaveEntity failed to register with WaveEventManager: {this.name}. DEACTIVATING!");
            gameObject.SetActive(false);
        }

    }


    private void OnDisable()
    {
        if (waveEventManager != null)
        {
            waveEventManager.Unregister_WaveEntity(this);
        }
    }

    private void OnDestroy()
    {
        if (WaveEventManager.Instance != null)
        {
            Debug.Log("Entity On Destroy");

            WaveEventManager.Instance.Unregister_WaveEntity(this);
        }
    }


}
