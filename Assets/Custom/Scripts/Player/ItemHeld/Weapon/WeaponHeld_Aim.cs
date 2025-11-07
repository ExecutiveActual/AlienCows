using System.Collections.Generic;
using UnityEngine;

public class WeaponHeld_Aim : MonoBehaviour
{

    [SerializeField] private HandSlot slot_Aim;    
    

    bool isAiming = false;


    private ItemHeld_SlerpGuide_Manager slerpGuide_Manager;


    private void Awake()
    {

        slerpGuide_Manager = GetComponent<ItemHeld_SlerpGuide_Manager>();

    }

    private void Start()
    {


    }


    private void OnAim()
    {

        if (isAiming)
        {
            slerpGuide_Manager.ResetSlerpGuideTarget();
            isAiming = false;
        }
        else
        {
            slerpGuide_Manager.SetSlerpGuideTarget(slot_Aim.transform);
            isAiming = true;
            BroadcastMessage("OnSafetyOff", SendMessageOptions.DontRequireReceiver);
        }

    }

    private void OnResetSlerpGuideTarget()
    {
        isAiming = false;
    }


}
