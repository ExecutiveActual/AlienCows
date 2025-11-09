using UnityEngine;
using UnityEngine.Events;

public class HitZone : MonoBehaviour
{

    private HealthManager healthManager;

    [SerializeField] private float damageMultiplier = 1f;



    private void Awake()
    {
        if (healthManager == null)
        {
            healthManager = GetComponentInParent<HealthManager>();
            if (healthManager == null)
            {
                healthManager = GetComponent<HealthManager>();
            }
        }
    }


    public void TakeDamage(float damage)
    {
        Debug.Log($"{this}, took {damage} damage!");

        healthManager.TakeDamage(damage * damageMultiplier);

    }
}
