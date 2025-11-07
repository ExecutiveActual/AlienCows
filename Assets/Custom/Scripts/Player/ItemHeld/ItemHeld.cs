using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class ItemHeld : MonoBehaviour
{
    // Backing fields — private, only for serialization
    [SerializeField] private string _ItemName = "ItemName Not Set";
    //[SerializeField] private HandSlot _defaultHandSlot;
    

    // Public/protected access — derived classes can use
    public string ItemName { get; private set; }
    public HandSlot DefaultHandSlot { get; private set; }

    // Only derived classes and this class need access
    protected PlayerController_ItemHeld playerController;

    public Camera_HandSlotManager HandSlotManager { get; private set; }


    public UnityEvent UE_OnInitialize;

    public UnityEvent UE_OnDeinitialize;


    public void Initialize(PlayerController_ItemHeld controller)
    {
        playerController = controller;
        playerController.OnPlayerInput += HandlePlayerInput;

        HandSlotManager = playerController.HandSlotManager;

        DefaultHandSlot = playerController.HandSlot_Default;

        UE_OnInitialize?.Invoke();
    }

    public void Deinitialize()
    {
        UE_OnDeinitialize?.Invoke();

        if (playerController != null)
            playerController.OnPlayerInput -= HandlePlayerInput;
        playerController = null;

    }

    protected virtual void HandlePlayerInput(InputAction.CallbackContext ctx)
    {
        // Base does nothing
    }

    private void Awake()
    {
        ItemName = _ItemName;
        //defaultHandSlot = _defaultHandSlot;
        
    }

    private void Start()
    {
        
    }

    private void OnDestroy() => Deinitialize();
}