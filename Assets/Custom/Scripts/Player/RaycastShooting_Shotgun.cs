using UnityEngine;

public class RaycastShooting_Shotgun : MonoBehaviour
{
    GunController gunController;
    [SerializeField] private Transform bulletSpawner;
    [SerializeField] private float damage_PerPellet = 12f;
    [SerializeField] private int pellet_Count = 12;
    [SerializeField] private float spread_Angle = 5f; // Total cone angle in degrees

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
        for (int i = 0; i < pellet_Count; i++)
        {
            Vector3 spreadDirection = GetSpreadDirection();
            SpawnPellet(spreadDirection);
        }
    }

    private Vector3 GetSpreadDirection()
    {
        // Random point inside a cone
        float angle = Random.Range(0f, spread_Angle);
        float randomCircle = Random.Range(0f, 360f);

        // Convert spherical to cartesian (in local space of bulletSpawner)
        Vector3 direction = Vector3.forward;
        direction = Quaternion.AngleAxis(angle, Vector3.up) * direction;
        direction = Quaternion.AngleAxis(randomCircle, Vector3.forward) * direction;

        // Transform direction from local (forward = +Z) to world space
        return bulletSpawner.TransformDirection(direction.normalized);
    }

    private void SpawnPellet(Vector3 direction)
    {
        // Debug ray to visualize spread
        Debug.DrawRay(bulletSpawner.position, direction * 100f, Color.red, 3f);

        RaycastHit hit;
        if (Physics.Raycast(bulletSpawner.position, direction, out hit))
        {
            HitZone target_HitZone = hit.transform.GetComponent<HitZone>();
            if (target_HitZone != null)
            {
                target_HitZone.TakeDamage(damage_PerPellet);
            }

            // Optional: Add impact effects here
            // Instantiate(hitEffect, hit.point, Quaternion.LookRotation(hit.normal));
        }
    }
}