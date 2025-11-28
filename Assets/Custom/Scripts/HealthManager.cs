using UnityEngine;
using UnityEngine.Events;

public class HealthManager : MonoBehaviour
{

    public float Health_Current { get; private set; }

    public float Health_Max { get; private set; }
    [SerializeField] private float _Health_Max = 100f;


    public UnityEvent<float> UE_OnTakeDamage;

    public UnityEvent UE_OnDeath;


    public bool isInvincible { get; private set; } = false;

    public bool canDie { get; private set; } = true;

    public bool isDead { get; private set; }



    private void Awake()
    {
        Health_Max = _Health_Max;
        Health_Current = Health_Max;
    }


    public void TakeDamage(float amount)
    {
        if (isDead || isInvincible)
            return;

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

        if (Health_Current <= 0f)
        {
            if (canDie && !isDead)
            {
                Death();
            }
        }

        UE_OnTakeDamage?.Invoke(amount);
    }


    public void SetCanDie(bool newValue)
    {
        canDie = newValue;

        //Checks if it should die immediately
        if (Health_Current <= 0f)
        {
            if (canDie && !isDead)
            {
                Death();
            }
        }
    }


    public void SetInvincible(bool newValue)
    {
        isInvincible = newValue;

        SetCanDie(!newValue);
    }


    private void Death()
    {
        //Debug.Log($"{this}, died!");
        isDead = true;
        UE_OnDeath?.Invoke();


        Destroy(gameObject, 0.1f);
    }

}
