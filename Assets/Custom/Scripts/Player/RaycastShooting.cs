using UnityEngine;

public class RaycastShooting : MonoBehaviour
{

    GunController gunController;


    [SerializeField] private Transform bulletSpawner;


    [SerializeField] private float damage = 1f;


    [SerializeField] private LayerMask layersToIgnore;



    private void Awake()
    {
        gunController = GetComponent<GunController>();
    }



    private void OnEnable()
    {
        gunController.UE_OnShoot.AddListener(Shoot);
    }
    private void OnDisable()
    {
        gunController.UE_OnShoot.RemoveListener(Shoot);
    }


    public void Shoot()
    {
        Debug.DrawRay(bulletSpawner.position, bulletSpawner.forward * 100f, Color.cyan, 0.1f);

        RaycastHit hit;

        if (Physics.Raycast(bulletSpawner.position, bulletSpawner.forward, out hit, Mathf.Infinity, ~layersToIgnore))
        {
            HitZone target_HitZone = hit.transform.GetComponent<HitZone>();
            if (target_HitZone != null)
            {
                target_HitZone.TakeHit(hit);
                target_HitZone.TakeDamage(damage);
            }
        }
    }


}
