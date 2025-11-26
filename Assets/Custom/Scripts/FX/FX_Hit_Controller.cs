using UnityEngine;

public class FX_Hit_Controller : MonoBehaviour
{
    ParticleSystem[] particles;

    void Awake()
    {
        particles = GetComponentsInChildren<ParticleSystem>();
    }

    public void PerformFX()
    {
        foreach (var ps in particles)
            ps.Play();

        // Optional: destroy after it finishes
        Destroy(gameObject, 3f);
    }
}