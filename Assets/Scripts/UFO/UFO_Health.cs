using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class HealthEvent : UnityEvent<UFO_Health> { }
[System.Serializable]
public class DamageEvent : UnityEvent<float> { }

public class UFO_Health : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f;
    public float CurrentHealth { get; private set; }

    [Header("VFX")]
    public GameObject brokenPrefab;
    public bool spawnBrokenPrefab = true;

    [Header("Audio")]
    public AudioClip deathSFX;
    public float deathVolume = 1f;

    [Header("Events")]
    public HealthEvent OnUFODestroyed;
    public DamageEvent OnUFODamaged;

    private AudioSource src;

    void Awake()
    {
        CurrentHealth = maxHealth;
        src = GetComponent<AudioSource>();
    }

    public void TakeDamage(float amt)
    {
        if (CurrentHealth <= 0) return;

        CurrentHealth -= amt;
        OnUFODamaged?.Invoke(amt);

        if (CurrentHealth <= 0)
        {
            CurrentHealth = 0;
            Die();
        }
    }

    void Die()
    {
        if (spawnBrokenPrefab && brokenPrefab)
        {
            Instantiate(brokenPrefab, transform.position, transform.rotation);
        }

        if (src && deathSFX)
        {
            src.PlayOneShot(deathSFX, deathVolume);
        }

        OnUFODestroyed?.Invoke(this);
        Destroy(gameObject);
    }
}
