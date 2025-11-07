using System.CodeDom.Compiler;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;   // for ReadOnlyArray

[RequireComponent(typeof(PlayerInput))]
public class PlayerController_ItemHeld : MonoBehaviour
{
    [Header("Assign your .inputactions asset here")]
    [SerializeField] private InputActionAsset inputActionsAsset;

    [Header("Current held item (set from inventory)")]
    [SerializeField] private ItemHeld itemHeld_Curr;

    [Header("Hand Slot")]
    [SerializeField] private HandSlot handSlot_Default;
    public HandSlot HandSlot_Default => handSlot_Default;


    // One event → all items listen to this
    public event System.Action<InputAction.CallbackContext> OnPlayerInput;

    private PlayerInput _playerInput;

    private void Awake()
    {
        _playerInput = GetComponent<PlayerInput>();

        // Optional: make sure the asset is assigned in the Inspector
        if (inputActionsAsset != null)
            _playerInput.actions = inputActionsAsset;
    }

    private void OnEnable()
    {
        // Subscribe to **every** action in the asset (all maps)
        foreach (var action in _playerInput.actions)
        {
            action.performed += Forward;
            action.started += Forward;
            action.canceled += Forward;
        }

        // If you only want the "Player" map, enable it explicitly:
        // _playerInput.SwitchCurrentActionMap("Player");
    }

    private void OnDisable()
    {
        foreach (var action in _playerInput.actions)
        {
            action.performed -= Forward;
            action.started -= Forward;
            action.canceled -= Forward;
        }
    }

    private void Forward(InputAction.CallbackContext ctx)
    {
        // Debug – you will see this for every button press, stick move, etc.
        //Debug.Log($"<color=cyan>[INPUT] {ctx.action.name} → {ctx.phase}</color>");

        if (itemHeld_Curr != null)
            OnPlayerInput?.Invoke(ctx);
    }

    // Public helper used by your inventory / equip system
    public void SetItemHeld(ItemHeld newItem) => itemHeld_Curr = newItem;

    public ItemHeld GetItemHeld() => itemHeld_Curr;

    public void ClearItemHeld()
    {
        Destroy(itemHeld_Curr.gameObject);
        itemHeld_Curr = null;
    }
}