using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class WeaponHeld_Aim : MonoBehaviour
{
    WeaponHeld weaponHeld;

    GunController gunController;

    private HandSlot slot_Aim;

    private ItemHeld_SlerpGuide_Manager slerpGuide_Manager;


    public bool isAiming { get; private set; }


    private void Awake()
    {

        weaponHeld = GetComponent<WeaponHeld>();
        gunController = GetComponent<GunController>();

        slerpGuide_Manager = GetComponent<ItemHeld_SlerpGuide_Manager>();

    }

    private void OnEnable()
    {
        weaponHeld.UE_OnInitialize.AddListener(Initialize);

        weaponHeld.UE_OnAim_Start.AddListener(Aim_Start);
        weaponHeld.UE_OnAim_Stop.AddListener(Aim_Stop);

        slerpGuide_Manager.UE_OnResetSlerpGuideTarget.AddListener(ResetSlerpGuideTarget);
    }

    private void OnDisable()
    {
        weaponHeld.UE_OnInitialize.RemoveListener(Initialize);

        weaponHeld.UE_OnAim_Start.RemoveListener(Aim_Start);
        weaponHeld.UE_OnAim_Stop.RemoveListener(Aim_Stop);

        slerpGuide_Manager.UE_OnResetSlerpGuideTarget.RemoveListener(ResetSlerpGuideTarget);
    }


    private void Initialize()
    {
        slot_Aim = weaponHeld.HandSlotManager.FindSlotByType(HandSlotType.Aim);
    }


    private void Aim_Start()
    {
        if (!gunController.isReloading)
        {
            if (!isAiming)
            {
                slerpGuide_Manager.SetSlerpGuideTarget(slot_Aim.transform);
                isAiming = true;
            }
        }
    }


    private void Aim_Stop()
    {
        if (isAiming)
        {
            slerpGuide_Manager.ResetSlerpGuideTarget();
            isAiming = false;
        }
    }


    private void ResetSlerpGuideTarget()
    {
        isAiming = false;
    }


}
