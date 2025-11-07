using UnityEngine;

public class WeaponHeld_Inspect : MonoBehaviour
{

    private ItemHeld_SlerpGuide_Manager slerpGuide_Manager;


    [SerializeField] private HandSlot handSlot_Inspect;


    bool isInspecting = false;




    private void Awake()
    {
        slerpGuide_Manager = GetComponent<ItemHeld_SlerpGuide_Manager>();
    }

    private void OnFire()
    {
        if (isInspecting)
        {

            EnterInspect(false);

        }
    }

    private void OnInspect()
    {

        if (isInspecting)
        {
            EnterInspect(false);
        }
        else
        {
            EnterInspect(true);
        }

    }

    private void EnterInspect(bool state)
    {

        if (state)
        {
            slerpGuide_Manager.SetSlerpGuideTarget(handSlot_Inspect.transform);
            isInspecting = true;
            BroadcastMessage("OnSafetyOn", SendMessageOptions.DontRequireReceiver);
        }
        else
        {
            slerpGuide_Manager.ResetSlerpGuideTarget();
            isInspecting = false;
            //BroadcastMessage("SafetyOff", SendMessageOptions.DontRequireReceiver);
        }

    }


    private void OnResetSlerpGuideTarget()
    {
        isInspecting = false;
    }


}