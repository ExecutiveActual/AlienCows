using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class ItemHeld : MonoBehaviour
{
    public string ItemName { get; private set; }
    [SerializeField] private string _ItemName = "ItemName Not Set";

    public HandSlot defaultHandSlot { get; private set; }
    [SerializeField] private HandSlot _defaultHandSlot;

    public DumbSlerpToTarget SlerpGuide { get; private set; }
    [SerializeField] private DumbSlerpToTarget _SlerpGuide;

    protected PlayerController_ItemHeld playerController;

    public void Initialize(PlayerController_ItemHeld controller)
    {
        playerController = controller;
        playerController.OnPlayerInput += HandlePlayerInput;
    }

    public void Deinitialize()
    {
        if (playerController != null)
        {
            playerController.OnPlayerInput -= HandlePlayerInput;
            playerController = null;
        }
    }

    // Virtual method — derived classes override this
    protected virtual void HandlePlayerInput(InputAction.CallbackContext context)
    {
        // Base does nothing
    }

    private void Awake()
    {
        ItemName = _ItemName;
        defaultHandSlot = _defaultHandSlot;
        SlerpGuide = _SlerpGuide;
    }

    private void Start()
    {
        SlerpGuide.transform.SetParent(null);
    }

    private void OnDestroy()
    {
        Deinitialize();
    }
}