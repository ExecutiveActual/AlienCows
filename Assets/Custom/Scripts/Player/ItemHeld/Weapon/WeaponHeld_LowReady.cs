using UnityEngine;

public class WeaponHeld_LowReady : MonoBehaviour
{

    [SerializeField] private HandSlot slot_LowReady;


    bool isLowReady = false;


    private ItemHeld_SlerpGuide_Manager slerpGuide_Manager;


    private void Awake()
    {

        slerpGuide_Manager = GetComponent<ItemHeld_SlerpGuide_Manager>();

    }

    private void Start()
    {


    }


    private void OnFire()
    {
        if (isLowReady)
        {
            EnterLowReady(false);
        }
    }

    private void OnLowReady()
    {

        if (isLowReady)
        {
            EnterLowReady(false);
        }
        else
        {
            EnterLowReady(true);
        }

    }

    private void EnterLowReady(bool state)
    {

        if (state)
        {
            slerpGuide_Manager.SetSlerpGuideTarget(slot_LowReady.transform);
            isLowReady = true;
            BroadcastMessage("OnSafetyOn", SendMessageOptions.DontRequireReceiver);
            Debug.Log("LowReady");
        }
        else
        {
            slerpGuide_Manager.ResetSlerpGuideTarget();
            isLowReady = false;
            //BroadcastMessage("SafetyOff", SendMessageOptions.DontRequireReceiver);
        }

    }

    private void OnResetSlerpGuideTarget()
    {
        isLowReady = false;
    }


}
