using UnityEngine;

public class RaycastShooting : MonoBehaviour
{

    GunController gunController;


    [SerializeField] private Transform bulletSpawner;


    [SerializeField] private float damage = 1f;


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
        Debug.DrawRay(bulletSpawner.position, bulletSpawner.forward * 100f, Color.red, 10f);


        RaycastHit hit;

        if (Physics.Raycast(bulletSpawner.position, bulletSpawner.forward, out hit))
        {

            HitZone target_HitZone = hit.transform.GetComponent<HitZone>();


            if (target_HitZone != null)
            {
                target_HitZone.TakeDamage(damage);
            }
        }
    }


}
