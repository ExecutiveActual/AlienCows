using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class ItemHeld : MonoBehaviour
{
    // Backing fields — private, only for serialization
    [SerializeField] private string _ItemName = "ItemName Not Set";
    //[SerializeField] private HandSlot _defaultHandSlot;
    [SerializeField] private DumbSlerpToTarget _SlerpGuide;

    // Public/protected access — derived classes can use
    public string ItemName { get; private set; }
    public HandSlot DefaultHandSlot { get; private set; }
    public DumbSlerpToTarget SlerpGuide { get; private set; }

    // Only derived classes and this class need access
    protected PlayerController_ItemHeld playerController;

    public void Initialize(PlayerController_ItemHeld controller)
    {
        playerController = controller;
        playerController.OnPlayerInput += HandlePlayerInput;

        DefaultHandSlot = playerController.HandSlot_Default;
    }

    public void Deinitialize()
    {
        if (playerController != null)
            playerController.OnPlayerInput -= HandlePlayerInput;
        playerController = null;

        Destroy(SlerpGuide?.gameObject);
    }

    protected virtual void HandlePlayerInput(InputAction.CallbackContext ctx)
    {
        // Base does nothing
    }

    private void Awake()
    {
        ItemName = _ItemName;
        //defaultHandSlot = _defaultHandSlot;
        SlerpGuide = _SlerpGuide;
    }

    private void Start()
    {
        if (SlerpGuide != null)
            SlerpGuide.transform.SetParent(null);
    }

    private void OnDestroy() => Deinitialize();
}