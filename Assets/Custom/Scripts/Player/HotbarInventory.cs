using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class HotbarInventory : MonoBehaviour
{

    [Header("References")]
    private PlayerController_ItemHeld playerController;


    private int currentHotbarIndex = 0;


    public WeaponHeld weapon_1 { get; private set; }
    public WeaponHeld weapon_2 { get; private set; }

    public ItemHeld item_3 { get; private set; }
    public ItemHeld item_4 { get; private set; }
    public ItemHeld item_5 { get; private set; }



    private void Awake()
    {
        if (playerController == null)
            playerController = GetComponent<PlayerController_ItemHeld>();

        GameManager_Singleton.Instance.GetComponent<GameManager_GiveInventory>().UE_OnInitializeInventory.AddListener(InitializeInventory);
    }



    public void InitializeInventory(HotbarInventoryItems newItems)
    {
        weapon_1 = newItems.weapon_1;
        weapon_2 = newItems.weapon_2;
        item_3 = newItems.item_3;
        item_4 = newItems.item_4;
        item_5 = newItems.item_5;

        SelectHotbarSlot(1);
    }


    // -----------------------------------------------------------------
    // 1. Unity will call this automatically (Send Messages)
    // -----------------------------------------------------------------
    private void OnInventoryItemSelect()
    {
        // -------------------------------------------------------------
        // 2. Find *which* key just fired the action
        // -------------------------------------------------------------
        // PlayerInput.current gives us the component that sent the message
        var playerInput = PlayerInput.GetPlayerByIndex(0);   // change index if needed
        if (playerInput == null) return;

        // Grab the action reference
        var action = playerInput.actions["Player/InventoryItemSelect"];
        if (action == null) return;

        // The control that triggered the *last* performed callback
        var control = action.activeControl;
        if (control == null) return;

        // Path is "/keyboard/1", "/keyboard/2", …
        string path = control.path;
        char keyChar = path[path.Length - 1];

        if (!char.IsDigit(keyChar)) return;

        int slotIndex = keyChar - '0';
        if (slotIndex < 1 || slotIndex > 5) return;

        // -------------------------------------------------------------
        // 3. Your hotbar logic
        // -------------------------------------------------------------
        SelectHotbarSlot(slotIndex);
    }

    // -----------------------------------------------------------------
    // Your hotbar implementation
    // -----------------------------------------------------------------
    private void SelectHotbarSlot(int index)
    {
        if (currentHotbarIndex == index)
            return;

        ItemHeld itemToEquip = GetItemHeldByIndex(index);

        if (itemToEquip == null)
            return;

        Debug.Log($"Hotbar slot {index} selected!");

        currentHotbarIndex = index;

        Equip(itemToEquip, playerController.HandSlot_Default.transform);
    }


    private ItemHeld GetItemHeldByIndex(int index)
    {
        return index switch
        {
            1 => weapon_1,
            2 => weapon_2,
            3 => item_3,
            4 => item_4,
            5 => item_5,
            _ => null
        };
    }


    public void Equip(ItemHeld prefab, Transform hand)
    {

        if (prefab == null) return; 

        // 1. Destroy old
        if (playerController.GetItemHeld())
            playerController.ClearItemHeld();

        // 2. Instantiate
        var go = Instantiate(prefab, hand);
        go.transform.SetParent(hand.parent);
        var item = go.GetComponent<ItemHeld>();

        // 3. Tell the controller
        playerController.SetItemHeld(item);

        // 4. Initialise the item (subscribes to OnPlayerInput)
        item.Initialize(playerController);
    }


}