using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class GameManager_GiveInventory : MonoBehaviour, IGameManagerModule
{
    //[SerializeField] private WeaponHeld starterWeapon;

    [SerializeField] private int starterWeaponID;

    [SerializeField] private SO_WeaponTable weaponTable;


    public WeaponHeld Weapon_1 { get; private set; }
    public WeaponHeld Weapon_2 { get; private set; }
    public WeaponHeld Weapon_3 { get; private set; }



    public UnityEvent<HotbarInventoryItems> UE_OnInitializeInventory;



    // ----------------------------------------------------------------------
    // These should be used by the UI as the slots get items assigned to them
    // ----------------------------------------------------------------------
    public void SetWeapon_1(int newWeaponID)
    {
        WeaponHeld newWeapon = weaponTable.GetWeaponFromID(newWeaponID);

        if (newWeapon == null)
        {
            Debug.LogError($"GameManager_GiveInventory: SetWeapon_1 - No weapon found with ID {newWeaponID}");
            
            newWeapon = weaponTable.GetWeaponFromID(starterWeaponID);
        }
        Weapon_1 = newWeapon;

    }
    public void SetWeapon_2(int newWeaponID)
    {
        WeaponHeld newWeapon = weaponTable.GetWeaponFromID(newWeaponID);

        Weapon_2 = newWeapon;
    }
    public void SetWeapon_3(int newWeaponID)
    {
        WeaponHeld newWeapon = weaponTable.GetWeaponFromID(newWeaponID);

        Weapon_3 = newWeapon;
    }


    public HotbarInventoryItems GetHotbarInventoryItems()
    {
        ImportHotbarFromSave();

        HotbarInventoryItems hotbarItems = new HotbarInventoryItems
        {
            weapon_1 = Weapon_1,
            weapon_2 = Weapon_2,
            weapon_3 = Weapon_3
        };
        return hotbarItems;
    }


    private void ImportHotbarFromSave()
    {

        GameManager_SaveSystem saveSystem = GameManager_Singleton.Instance.GetComponent<GameManager_SaveSystem>();

        if (saveSystem != null)
        {

            Weapon_1 = weaponTable.GetWeaponFromID(saveSystem.PlayerData_Curr.SavedHotbar[0]);

            Weapon_2 = weaponTable.GetWeaponFromID(saveSystem.PlayerData_Curr.SavedHotbar[1]);

            Weapon_3 = weaponTable.GetWeaponFromID(saveSystem.PlayerData_Curr.SavedHotbar[2]);


        }

    }



    public void OnInitializeModule()
    {
        //Debug.Log("GameManager_GiveInventory: OnInitializeModule called.");

        //if (GameManager_Singleton.Instance.GetComponent<GameManager_SaveSystem>().PlayerData_Curr.NightNumber == 0)
        //{
        //    SetWeapon_1(starterWeaponID);

        //    //Debug.Log($"GameManager_GiveInventory: Starter weapon assigned to Weapon_1 = {Weapon_1}");
        //}
    }


    public void InventoryCheckIn()
    {
        UE_OnInitializeInventory?.Invoke(GetHotbarInventoryItems());
    }

    
}


public struct HotbarInventoryItems
{
    public WeaponHeld weapon_1;
    public WeaponHeld weapon_2;
    public WeaponHeld weapon_3;
}
