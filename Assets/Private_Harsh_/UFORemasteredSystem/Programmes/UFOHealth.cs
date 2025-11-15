using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// UFOHealth
/// - Simple, robust health component with UnityEvents.
/// - Reports damage, death, and percent-changed events.
/// - Replace mesh with broken model on death, play SFX, and destroy after delay.
/// - If damaged during abduction, it is expected to call abductor.InterruptAbduction() from the controller (controller subscribes).
/// </summary>
[DisallowMultipleComponent]
public class UFOHealth : MonoBehaviour
{
    public enum EnemyType { UFO, Drone, Boss }

    [Header("Health Settings")]
    [Tooltip("Maximum health.")]
    public float maxHealth = 100f;

    [Tooltip("Starting health fraction (0..1).")]
    [Range(0f, 1f)]
    public float startFraction = 1f;

    [Header("Auto-destroy on death")]
    [Tooltip("Seconds to wait after death before destroying the object.")]
    public float destroyDelay = 2f;

    [Header("Broken Visuals (optional)")]
    [Tooltip("Optional broken model prefab to instantiate on death. If null, original model remains.")]
    public GameObject brokenModelPrefab;

    [Header("Sound")]
    [Tooltip("Sound to play when destroyed.")]
    public AudioClip destroyedSfx;

    [Tooltip("Volume for destroyed SFX.")]
    [Range(0f, 1f)]
    public float destroyedSfxVolume = 1f;

    [Header("Runtime (read-only)")]
    [Tooltip("Current health value.")]
    public float currentHealth { get; private set; }

    // -----------------------
    // UnityEvents
    // -----------------------
    [System.Serializable] public class FloatEvent : UnityEvent<float> { }
    [System.Serializable] public class NoArgEvent : UnityEvent { }

    [Header("Events")]
    [Tooltip("Invoked when the UFO takes damage; param = damage amount.")]
    public FloatEvent onDamageReceived;

    [Tooltip("Invoked when UFO dies.")]
    public NoArgEvent onDeath;

    [Tooltip("Invoked when health percent (0..1) changes; param = new percent.")]
    public FloatEvent onHealthPercentChanged;

    [Tooltip("Invoked when health drops below a certain threshold (not used internally but exposed).")]
    public FloatEvent onHealthBelowThreshold;

    private bool isDead = false;

    private void Start()
    {
        currentHealth = Mathf.Clamp(maxHealth * startFraction, 0f, maxHealth);
        onHealthPercentChanged?.Invoke(GetHealthPercent());
    }

    /// <summary>
    /// Apply damage to UFO. Public, called by player weapons.
    /// </summary>
    public void ApplyDamage(float amount)
    {
        if (isDead) return;
        if (amount <= 0f) return;

        currentHealth -= amount;
        onDamageReceived?.Invoke(amount);
        onHealthPercentChanged?.Invoke(GetHealthPercent());

        if (currentHealth <= 0f)
        {
            currentHealth = 0f;
            Die();
        }
    }

    /// <summary>
    /// Get health 0..1
    /// </summary>
    public float GetHealthPercent()
    {
        return (maxHealth <= 0f) ? 0f : (currentHealth / maxHealth);
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;
        // play broken model & sfx
        if (brokenModelPrefab != null)
        {
            var broken = Instantiate(brokenModelPrefab, transform.position, transform.rotation);
            // optional: parent to this object so it moves with debris before destroy; but we'll just spawn detached.
        }

        if (destroyedSfx != null)
        {
            var audioGO = new GameObject("UFO_Destroyed_SFX");
            audioGO.transform.position = transform.position;
            var src = audioGO.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.spatialBlend = 1f;
            src.PlayOneShot(destroyedSfx, destroyedSfxVolume);
            Destroy(audioGO, 3f);
        }

        onDeath?.Invoke();

        // Optionally disable visuals immediately to show broken model only
        // Try disabling renderers
        TryDisableRenderers();

        // Destroy object after delay
        Destroy(gameObject, destroyDelay);
    }

    private void TryDisableRenderers()
    {
        var renderers = GetComponentsInChildren<Renderer>();
        foreach (var r in renderers) r.enabled = false;
    }
}
