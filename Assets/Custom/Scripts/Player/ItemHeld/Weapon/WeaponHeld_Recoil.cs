using UnityEngine;

public class WeaponHeld_Recoil : MonoBehaviour
{


    [SerializeField] private float recoilX = 1.6f;
    [SerializeField] private float recoilY = -9f;
    [SerializeField] private float recoilZ = -0.07f;



    private void OnRecoil()
    {

        transform.Translate(Vector3.forward * recoilZ, Space.Self);
        transform.Rotate(Vector3.right, recoilY, Space.Self);
        transform.Rotate(Vector3.up, recoilX, Space.Self);

    }

}
