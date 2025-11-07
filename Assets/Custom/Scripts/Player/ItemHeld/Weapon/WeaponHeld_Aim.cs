using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class WeaponHeld_Aim : MonoBehaviour
{

    private HandSlot slot_Aim;


    WeaponHeld weaponHeld;


    private ItemHeld_SlerpGuide_Manager slerpGuide_Manager;


    private void Awake()
    {

        weaponHeld = GetComponent<WeaponHeld>();

        slerpGuide_Manager = GetComponent<ItemHeld_SlerpGuide_Manager>();

    }

    private void OnEnable()
    {
        weaponHeld.UE_OnInitialize.AddListener(Initialize);

        weaponHeld.UE_OnAim_Start.AddListener(Aim_Start);
        weaponHeld.UE_OnAim_Stop.AddListener(Aim_Stop);
    }

    private void OnDisable()
    {
        weaponHeld.UE_OnInitialize.RemoveListener(Initialize);

        weaponHeld.UE_OnAim_Start.RemoveListener(Aim_Start);
        weaponHeld.UE_OnAim_Stop.RemoveListener(Aim_Stop);
    }


    private void Initialize()
    {
        slot_Aim = weaponHeld.HandSlotManager.FindSlotByType(HandSlotType.Aim);
    }


    private void Aim_Start()
    {

        slerpGuide_Manager.SetSlerpGuideTarget(slot_Aim.transform);
        BroadcastMessage("OnSafetyOff", SendMessageOptions.DontRequireReceiver);

    }

    private void Aim_Stop()
    {
        slerpGuide_Manager.ResetSlerpGuideTarget();
    }

    //private void OnResetSlerpGuideTarget()
    //{
    //}


}
