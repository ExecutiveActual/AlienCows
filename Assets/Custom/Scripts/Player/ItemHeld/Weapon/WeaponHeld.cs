using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponHeld : ItemHeld
{
    protected override void HandlePlayerInput(InputAction.CallbackContext context)
    {
        switch (context.action.name)
        {
            case "Fire":
                if (context.performed) Fire();
                break;

            case "Aim":
                if (context.started) StartAiming();
                else if (context.canceled) StopAiming();
                break;

            case "Reload":
                if (context.performed) Reload();
                break;
        }
    }

    private void Fire() => Debug.Log("Bang!");
    private void StartAiming() => Debug.Log("Aiming...");
    private void StopAiming() => Debug.Log("Stopped aiming.");
    private void Reload() => Debug.Log("Reloading...");

}
