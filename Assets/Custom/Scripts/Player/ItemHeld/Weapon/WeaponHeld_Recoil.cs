using UnityEngine;

public class WeaponHeld_Recoil : MonoBehaviour
{

    GunController gunController;

    [SerializeField] private float recoilX = 1.6f;
    [SerializeField] private float recoilY = -9f;
    [SerializeField] private float recoilZ = -0.07f;


    private void Awake()
    {
        gunController = GetComponent<GunController>();
    }

    private void OnEnable()
    {
        gunController.UE_OnRecoil.AddListener(Recoil);
    }

    private void OnDisable()
    {
        gunController.UE_OnRecoil.RemoveListener(Recoil);
    }

    private void Recoil()
    {

        transform.Translate(Vector3.forward * recoilZ, Space.Self);
        transform.Rotate(Vector3.right, recoilY, Space.Self);
        transform.Rotate(Vector3.up, recoilX, Space.Self);

    }

}
