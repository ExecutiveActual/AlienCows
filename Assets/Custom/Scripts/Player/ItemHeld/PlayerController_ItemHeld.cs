using Unity.VisualScripting;
using UnityEngine;

public class PlayerController_ItemHeld : MonoBehaviour
{
    private PlayerInputActions inputActions;

    [SerializeField] private ItemHeld itemHeld_Curr;


    private void Awake()
    {
        inputActions = new PlayerInputActions();
    }


    private void OnEnable()
    {
        inputActions.Player.Enable();
    }

    private void OnDisable()
    {
        inputActions.Player.Disable();
    }



    private void OnAim()
    {
        if (itemHeld_Curr != null)
        {
            itemHeld_Curr.SendMessage("OnAim", SendMessageOptions.DontRequireReceiver);
        }
    }

    private void OnFire()
    {
        if (itemHeld_Curr != null)
        {
            itemHeld_Curr.SendMessage("OnFire", SendMessageOptions.DontRequireReceiver);
        }
    }

    private void OnInspect()
    {
        if (itemHeld_Curr != null)
        {
            itemHeld_Curr.SendMessage("OnInspect", SendMessageOptions.DontRequireReceiver);
        }
    }

    private void OnLowReady()
    {
        if (itemHeld_Curr != null)
        {
            itemHeld_Curr.SendMessage("OnLowReady", SendMessageOptions.DontRequireReceiver);
        }


    }
}
