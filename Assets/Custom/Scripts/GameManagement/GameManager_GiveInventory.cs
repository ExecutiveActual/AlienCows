using UnityEngine;
using UnityEngine.Events;

public class GameManager_GiveInventory : MonoBehaviour
{
    [SerializeField] private WeaponHeld starterWeapon;


    public WeaponHeld Weapon_1 { get; private set; }
    public WeaponHeld Weapon_2 { get; private set; }

    public ItemHeld Item_3 { get; private set; }
    public ItemHeld Item_4 { get; private set; }
    public ItemHeld Item_5 { get; private set; }



    public UnityEvent<HotbarInventoryItems> UE_OnInitializeInventory;



    // ----------------------------------------------------------------------
    // These should be used by the UI as the slots get items assigned to them
    // ----------------------------------------------------------------------
    public void SetWeapon_1(WeaponHeld newWeapon)
    {
        Weapon_1 = newWeapon;
    }
    public void SetWeapon_2(WeaponHeld newWeapon)
    {
        Weapon_2 = newWeapon;
    }
    public void SetItem_3(ItemHeld newItem)
    {
        Item_3 = newItem;
    }
    public void SetItem_4(ItemHeld newItem)
    {
        Item_4 = newItem;
    }
    public void SetItem_5(ItemHeld newItem)
    {
        Item_5 = newItem;
    }


    public HotbarInventoryItems GetHotbarInventoryItems()
    {
        HotbarInventoryItems hotbarItems = new HotbarInventoryItems
        {
            weapon_1 = Weapon_1,
            weapon_2 = Weapon_2,
            item_3 = Item_3,
            item_4 = Item_4,
            item_5 = Item_5
        };
        return hotbarItems;
    }



    private void Start()
    {

        if (GameManager_Singleton.Instance.GetComponent<GameManager_NightCounter>().currentNight == 1)
        {
            Weapon_1 = starterWeapon;
        }

        UE_OnInitializeInventory?.Invoke(GetHotbarInventoryItems());
    }


}


public struct HotbarInventoryItems
{
    public WeaponHeld weapon_1;
    public WeaponHeld weapon_2;
    public ItemHeld item_3;
    public ItemHeld item_4;
    public ItemHeld item_5;
}