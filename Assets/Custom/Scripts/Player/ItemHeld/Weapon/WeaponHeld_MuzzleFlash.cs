using System.Collections;
using UnityEngine;

public class WeaponHeld_MuzzleFlash : MonoBehaviour
{

    GunController gunController;

    [SerializeField] private MuzzleFlash_Controller muzzleFlash_Controller;

    [SerializeField] private Transform muzzleFlashSpawnerTransform;







    private void Start()
    {
        
        gunController = GetComponent<GunController>();

        muzzleFlash_Controller = GetComponentInChildren<MuzzleFlash_Controller>();

        muzzleFlash_Controller.transform.SetParent(null, true);

        gunController.UE_OnShoot.AddListener(MuzzleFlash);

    }


    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {
        gunController.UE_OnShoot.RemoveListener(MuzzleFlash);
    }



    public void MuzzleFlash()
    {

        Debug.Log("Muzzle");

        muzzleFlash_Controller.transform.position = muzzleFlashSpawnerTransform.position;

        muzzleFlash_Controller.transform.rotation = muzzleFlashSpawnerTransform.rotation;

        muzzleFlash_Controller.PerformMuzzleFlash();

    }



}
