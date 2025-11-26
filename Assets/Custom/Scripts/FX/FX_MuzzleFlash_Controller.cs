using System.Collections.Generic;
using UnityEngine;

public class FX_MuzzleFlash_Controller : MonoBehaviour
{
    [SerializeField] private List<ParticleSystem> particleSystems;



    private void Awake()
    {

        particleSystems = new List<ParticleSystem>(GetComponentsInChildren<ParticleSystem>());

    }


    private void Start()
    {
        foreach (ParticleSystem ps in particleSystems)
        {
            ps.Stop();
        }
    }




    public void PerformMuzzleFlash()
    {
        foreach (ParticleSystem ps in particleSystems)
        {
            ps.Stop();
        }

        foreach (ParticleSystem ps in particleSystems)
        {
            ps.Play();
        }

    }
}
