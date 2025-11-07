using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController_ItemHeld : MonoBehaviour
{
    private PlayerInputActions inputActions;

    [SerializeField] private ItemHeld itemHeld_Curr;

    public event Action<InputAction.CallbackContext> OnPlayerInput;

    private void Awake()
    {
        inputActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        inputActions.Player.Enable();
        SubscribeToAllActions(inputActions.Player);
    }

    private void OnDisable()
    {
        if (inputActions != null)
        {
            UnsubscribeFromAllActions(inputActions.Player);
            inputActions.Player.Disable();
        }
    }

    private void SubscribeToAllActions(PlayerInputActions.PlayerActions playerActions)
    {
        var actionFields = typeof(PlayerInputActions.PlayerActions)
            .GetFields(BindingFlags.Public | BindingFlags.Instance)
            .Where(f => f.FieldType == typeof(InputAction));

        foreach (var field in actionFields)
        {
            var action = (InputAction)field.GetValue(playerActions);
            action.performed += OnInputPerformed;
            action.started += OnInputPerformed;
            action.canceled += OnInputPerformed;
        }
    }

    private void UnsubscribeFromAllActions(PlayerInputActions.PlayerActions playerActions)
    {
        var actionFields = typeof(PlayerInputActions.PlayerActions)
            .GetFields(BindingFlags.Public | BindingFlags.Instance)
            .Where(f => f.FieldType == typeof(InputAction));

        foreach (var field in actionFields)
        {
            var action = (InputAction)field.GetValue(playerActions);
            action.performed -= OnInputPerformed;
            action.started -= OnInputPerformed;
            action.canceled -= OnInputPerformed;
        }
    }

    private void OnInputPerformed(InputAction.CallbackContext context)
    {
        if (itemHeld_Curr != null)
        {
            OnPlayerInput?.Invoke(context);
        }
    }

    public void SetHeldItem(ItemHeld newItem)
    {
        itemHeld_Curr = newItem;
    }
}