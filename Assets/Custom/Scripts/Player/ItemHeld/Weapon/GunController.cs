using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class GunController : MonoBehaviour
{
    WeaponHeld weaponHeld;


    [SerializeField] private int magazine_Capacity = 8;
    private int magazine_AmmoCurrent;

    [SerializeField] private float reload_Duration = 1.2f;



    public UnityEvent UE_OnShoot;

    public UnityEvent UE_OnRecoil;


    public UnityEvent UE_OnReload_Start;

    public UnityEvent UE_OnReload_End;


    public bool isSafe { get; private set; }

    public bool isReloading { get; private set; }


    Coroutine reloadCoroutine;



    private void Awake()
    {
        weaponHeld = GetComponent<WeaponHeld>();
    }

    private void OnEnable()
    {
        weaponHeld.UE_OnFire.AddListener(Fire);
        weaponHeld.UE_OnReload.AddListener(Reload);
    }

    private void OnDisable()
    {
        weaponHeld.UE_OnFire.RemoveListener(Fire);
        weaponHeld.UE_OnReload.RemoveListener(Reload);
    }


    private void Start()
    {
        magazine_AmmoCurrent = magazine_Capacity;
    }


    private void Fire()
    {

        if (!isSafe)
        {
            if (magazine_AmmoCurrent > 0)
            {
                Shoot();
            }
        }
        else
        {
            SafetyOff();
        }

    }


    private void Reload()
    {
        if (magazine_AmmoCurrent < magazine_Capacity)
        {

            if (!isReloading)
            {
                reloadCoroutine = StartCoroutine(ReloadRoutine());
            }

        }
    }


    private IEnumerator ReloadRoutine()
    {
        SafetyOn();

        isReloading = true;

        UE_OnReload_Start?.Invoke();

        yield return new WaitForSeconds(reload_Duration);

        UE_OnReload_End?.Invoke();

        magazine_AmmoCurrent = magazine_Capacity;
        isReloading = false;
        SafetyOff();

    }



    private void SafetyOn()
    {
        isSafe = true;

        Debug.Log("Safety On");
    }


    private void SafetyOff()
    {
        isSafe = false;

        Debug.Log("Safety Off");
    }



    private void Shoot()
    {
        //Debug.Log("Bang!");

        magazine_AmmoCurrent--;


        UE_OnShoot?.Invoke();

        UE_OnRecoil?.Invoke();
    }


    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 200, 20), "Ammo: " + magazine_AmmoCurrent);
    }

}
