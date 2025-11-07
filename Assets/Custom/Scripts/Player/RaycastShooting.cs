using UnityEngine;

public class RaycastShooting : MonoBehaviour
{

    [SerializeField] private Transform bulletSpawner;


    [SerializeField] private float damage = 1f;



    private void OnShoot()
    {
        Shoot();
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
