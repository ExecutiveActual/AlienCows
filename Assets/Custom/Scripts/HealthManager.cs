using UnityEngine;
using UnityEngine.Events;

public class HealthManager : MonoBehaviour
{

    public float Health_Current { get; private set; }

    public float Health_Max { get; private set; }
    [SerializeField] private float _Health_Max = 100f;


    public UnityEvent<float> UE_OnTakeDamage;

    public UnityEvent UE_OnDeath;





    private void Awake()
    {
        Health_Max = _Health_Max;
        Health_Current = Health_Max;
    }


    public void TakeDamage(float amount)
    {
        Debug.Log($"{this}, took {amount} damage!");

        UE_OnTakeDamage?.Invoke(amount);
    }

}
