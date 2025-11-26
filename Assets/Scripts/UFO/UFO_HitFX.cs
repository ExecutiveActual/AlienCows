using UnityEngine;

public class UFO_HitFX : MonoBehaviour
{
    HitZone hitZone;
    [SerializeField] private GameObject hitEffectPrefab; // ← Change to GameObject for simplicity first

    [Tooltip("The Direction faced by the HitFX. (0,0,0) returns Vector3.down")]
    [SerializeField] private Vector3 hitEffectLookRotation = new Vector3(0, -1, 0);

    private void Awake()
    {
        hitZone = GetComponent<HitZone>();
    }

    private void Start()
    {
        if (hitZone != null)
            hitZone.UE_OnTakeHit.AddListener(PlayHitFX);
    }

    private void OnDisable()
    {
        if (hitZone != null)
            hitZone.UE_OnTakeHit.RemoveListener(PlayHitFX);
    }

    private void PlayHitFX(RaycastHit hit)
    {
        Debug.Log($"Playing UFO Hit FX at {hit.point} on {hit.collider.name}");

        // Safety check — this is the #1 reason people see nothing
        if (hitEffectPrefab == null)
        {
            Debug.LogError("[UFO_HitFX] hitEffectPrefab is NULL! Assign it in the Inspector!", this);
            return;
        }

        Vector3 pos = hit.point; // + hit.normal * 0.01f

        Quaternion rot;

        if (hitEffectLookRotation != Vector3.zero)
        {
            rot = Quaternion.LookRotation(hitEffectLookRotation);
        }
        else
        {
            rot = Quaternion.LookRotation(Vector3.down);
        }


        GameObject fx = Instantiate(hitEffectPrefab, pos, rot);

        // IMPORTANT: Parent it so it moves with moving objects and gets destroyed with the UFO
        fx.transform.SetParent(hit.transform);

        // If your prefab has a ParticleSystem that does NOT play on awake:
        var ps = fx.GetComponent<ParticleSystem>();
        if (ps != null)
            ps.Play();

        // Or if it has multiple:
        var allPS = fx.GetComponentsInChildren<ParticleSystem>();
        foreach (var p in allPS)
            p.Play();
    }
}