using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class WeaponHeld : ItemHeld
{

    PlayerController_Camera cameraController;


    [Header("Input Events")]

    public UnityEvent UE_OnFire;

    public UnityEvent UE_OnAim_Start;

    public UnityEvent UE_OnAim_Stop;

    public UnityEvent UE_OnReload;


    // ————————————————————————————
    // Player Input Handling
    // ————————————————————————————

    protected override void HandlePlayerInput(InputAction.CallbackContext ctx)
    {
        switch (ctx.action.name)
        {
            case "Fire" when ctx.performed:
                Fire();
                break;

            case "Aim" when ctx.started:
                StartAiming();
                break;

            case "Aim" when ctx.canceled:
                StopAiming();
                break;

            case "Reload" when ctx.performed:
                Reload();
                break;
        }
    }

    // ————————————————————————————
    // Weapon Behavior Methods
    // ————————————————————————————

    protected virtual void Fire()
    {
        //Debug.Log($"<color=red>[{ItemName}] BANG!</color>");
        // Add: spawn bullet, play sound, recoil, etc.
        UE_OnFire?.Invoke();
    }

    protected virtual void StartAiming()
    {
        //Debug.Log($"<color=yellow>[{ItemName}] Aiming down sights...</color>");
        // Add: FOV zoom, sway reduction, ADS animation

        UE_OnAim_Start?.Invoke();
    }

    protected virtual void StopAiming()
    {
        //Debug.Log($"<color=yellow>[{ItemName}] Stopped aiming.</color>");
        // Add: reset FOV, re-enable hipfire 

        UE_OnAim_Stop?.Invoke();
    }

    protected virtual void Reload()
    {
        //Debug.Log($"<color=orange>[{ItemName}] Reloading...</color>");
        // Add: play reload anim, refill ammo

        StopAiming(); // Typically, aiming is canceled when reloading

        UE_OnReload?.Invoke();
    }
}