using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponHeld : ItemHeld
{
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
        Debug.Log($"<color=red>[{ItemName}] BANG!</color>");
        // Add: spawn bullet, play sound, recoil, etc.
    }

    protected virtual void StartAiming()
    {
        Debug.Log($"<color=yellow>[{ItemName}] Aiming down sights...</color>");
        // Add: FOV zoom, sway reduction, ADS animation
    }

    protected virtual void StopAiming()
    {
        Debug.Log($"<color=yellow>[{ItemName}] Stopped aiming.</color>");
        // Add: reset FOV, re-enable hipfire
    }

    protected virtual void Reload()
    {
        Debug.Log($"<color=orange>[{ItemName}] Reloading...</color>");
        // Add: play reload anim, refill ammo
    }
}