using System;
using UnityEngine;

public class UFO_HitFX : MonoBehaviour
{

    HitZone hitZone;

    [SerializeField] private GameObject hitEffectPrefab;


    private void Awake()
    {
        hitZone = GetComponent<HitZone>();
    }

    private void Start()
    {
        hitZone.UE_OnTakeHit.AddListener(PlayHitFX);
    }

    private void OnDisable()
    {
        hitZone.UE_OnTakeHit.RemoveListener(PlayHitFX);
    }



    private void PlayHitFX(RaycastHit hit)
    {
        Vector3 newFXPosition = hit.point + hit.normal * 0.01f;

        Quaternion rotation = Quaternion.LookRotation(-hit.normal);

        ParticleSystem ps = Instantiate(hitEffectPrefab, newFXPosition, rotation, hit.transform).GetComponent<ParticleSystem>();

        ps.Play();
    }
}
