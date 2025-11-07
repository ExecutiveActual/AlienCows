using UnityEngine;

public class GunController : MonoBehaviour
{
    
    [SerializeField] private int magazine_Capacity = 8;
    private int magazine_AmmoCurrent;



    bool isSafe;



    private void Awake()
    {
    }


    private void Start()
    {
        magazine_AmmoCurrent = magazine_Capacity;
    }


    private void OnFire()
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
            OnSafetyOff();
        }

    }


    private void OnSafetyOn()
    {
        isSafe = true;

        Debug.Log("Safety On");
    }


    private void OnSafetyOff()
    {
        isSafe = false;
        Debug.Log("Safety Off");
    }



    private void Shoot()
    {
        //Debug.Log("Bang!");

        magazine_AmmoCurrent--;


        BroadcastMessage("OnShoot", SendMessageOptions.DontRequireReceiver);

        BroadcastMessage("OnRecoil", SendMessageOptions.DontRequireReceiver);
    }


    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 200, 20), "Ammo: " + magazine_AmmoCurrent);
    }

}
