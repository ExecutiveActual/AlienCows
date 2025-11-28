using UnityEngine;

public class UFO_WaveRegistry : MonoBehaviour
{

    private Spawner_UFO registeredSpawner;

    HealthManager healthManager;






    private void HandleDeath()
    {
        Unregister(registeredSpawner);
    }



    public void Register(Spawner_UFO spawner)
    {
        spawner.RegisterUFO(this);

        registeredSpawner = spawner;

        healthManager = GetComponent<HealthManager>();
        healthManager.UE_OnDeath.AddListener(HandleDeath);
    }

    public void Unregister(Spawner_UFO spawner)
    {
        spawner.UnregisterUFO(this);

        healthManager.UE_OnDeath.RemoveListener(HandleDeath);
    }


    private void OnDestroy()
    {
        if (registeredSpawner != null)
        {
            Unregister(registeredSpawner);
        }
    }

}
