using UnityEngine;
using UnityEngine.Events;

public class HealthManager : MonoBehaviour
{

    public float Health_Current { get; private set; }

    public float Health_Max { get; private set; }
    [SerializeField] private float _Health_Max = 100f;


    public UnityEvent<float> UE_OnTakeDamage;

    public UnityEvent UE_OnDeath;


    public bool isDead { get; private set; }



    private void Awake()
    {
        Health_Max = _Health_Max;
        Health_Current = Health_Max;
    }


    public void TakeDamage(float amount)
    {
        if (isDead)
            return;

        if (Health_Current <= 0f)
        {
            if (!isDead)
            {
                Death();
            }
            return;
        }


        if (amount > Health_Current)
        {
            Health_Current = 0f;

            Death();
        }
        else
        {
            Health_Current -= amount;
            //Debug.Log($"{this}, took {amount} damage, health = {Health_Current}");
        }

        UE_OnTakeDamage?.Invoke(amount);
    }

    private void Death()
    {
        //Debug.Log($"{this}, died!");
        isDead = true;
        UE_OnDeath?.Invoke();

        gameObject.SetActive(false);
    }

}
