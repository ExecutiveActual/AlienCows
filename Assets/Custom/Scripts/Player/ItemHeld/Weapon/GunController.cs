using UnityEngine;
using UnityEngine.Events;

public class GunController : MonoBehaviour
{
    
    [SerializeField] private int magazine_Capacity = 8;
    private int magazine_AmmoCurrent;

    WeaponHeld weaponHeld;


    public UnityEvent UE_OnShoot;

    public UnityEvent UE_OnRecoil;


    bool isSafe;



    private void Awake()
    {
        weaponHeld = GetComponent<WeaponHeld>();
    }

    private void OnEnable()
    {
        weaponHeld.UE_OnFire.AddListener(Fire);
    }

    private void OnDisable()
    {
        weaponHeld.UE_OnFire.RemoveListener(Fire);
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

        //BroadcastMessage("OnShoot", SendMessageOptions.DontRequireReceiver);

        //BroadcastMessage("OnRecoil", SendMessageOptions.DontRequireReceiver);
    }


    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 200, 20), "Ammo: " + magazine_AmmoCurrent);
    }

}
