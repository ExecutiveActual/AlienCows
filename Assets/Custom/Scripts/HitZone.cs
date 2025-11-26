using UnityEngine;
using UnityEngine.Events;

public class HitZone : MonoBehaviour
{

    private HealthManager healthManager;

    [SerializeField] private float damageMultiplier = 1f;

    public UnityEvent<RaycastHit> UE_OnTakeHit;



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


    public void TakeHit(RaycastHit hit_Incoming)
    {

        Debug.Log($"{this} took a hit");

        UE_OnTakeHit?.Invoke(hit_Incoming);

    }

}
