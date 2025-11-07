using UnityEngine;

public class BasicInventory : MonoBehaviour
{

    [Header("References")]
    private PlayerController_ItemHeld playerController;

    [SerializeField] private ItemHeld startingItem;


    private void Awake()
    {
        if (playerController == null)
            playerController = GetComponent<PlayerController_ItemHeld>();
    }



    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (startingItem != null)
            {
                Equip(startingItem, playerController.HandSlot_Default.transform);
            }
        }
    }




    public void Equip(ItemHeld prefab, Transform hand)
    {
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
