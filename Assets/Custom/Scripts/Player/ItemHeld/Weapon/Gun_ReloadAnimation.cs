using System;
using UnityEngine;

public class Gun_ReloadAnimation : MonoBehaviour
{

    private HandSlot slot_Lowered;

    private GunController gunController;

    private WeaponHeld weaponHeld;

    private ItemHeld_SlerpGuide_Manager slerpGuide_Manager;


    private void Awake()
    {
        gunController = GetComponent<GunController>();

        weaponHeld = GetComponent<WeaponHeld>();

        slerpGuide_Manager = GetComponent<ItemHeld_SlerpGuide_Manager>();
    }


    private void OnEnable()
    {
        weaponHeld.UE_OnInitialize.AddListener(Initialize);

        gunController.UE_OnReload_Start.AddListener(Reload_Start);
        gunController.UE_OnReload_End.AddListener(Reload_End);
    }

    private void OnDisable()
    {
        weaponHeld.UE_OnInitialize.RemoveListener(Initialize);

        gunController.UE_OnReload_Start.RemoveListener(Reload_Start);
        gunController.UE_OnReload_End.RemoveListener(Reload_End);
    }




    private void Initialize()
    {
        slot_Lowered = weaponHeld.HandSlotManager.FindSlotByType(HandSlotType.Lowered);
    }

    private void Reload_Start()
    {
        slerpGuide_Manager.ResetSlerpGuideTarget();

        slerpGuide_Manager.SetSlerpGuideTarget(slot_Lowered.transform);

    }

    private void Reload_End()
    {
        slerpGuide_Manager.ResetSlerpGuideTarget();
    }

    
}
