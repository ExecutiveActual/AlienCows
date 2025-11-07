using UnityEngine;

public class ItemHeld_SlerpGuide_Manager : MonoBehaviour
{
    
    private ItemHeld itemHeld;

    private DumbSlerpToTarget this_DumbSlerpToTarget;


    [Header("Slerp Guide Movement Settings")]

    [Tooltip("How many seconds it takes to reach ~63% of the distance (position).")]
    [Range(0.001f, 2f)] public float positionSmoothTime = 0.012f;

    [Tooltip("How many seconds it takes to reach ~63% of the rotation (degrees).")]
    [Range(0.001f, 2f)] public float rotationSmoothTime = 0.052f;



    public DumbSlerpToTarget GetSlerpGuide()
    {
        return itemHeld.SlerpGuide;
    }


    private void Awake()
    {
        itemHeld = GetComponent<ItemHeld>();

        this_DumbSlerpToTarget = GetComponent<DumbSlerpToTarget>();
    }


    private void Start()
    {
        
        // Set This SlerpTarget
        this_DumbSlerpToTarget.SetSlerpTargetTransform(GetSlerpGuide().transform);

        // Set SlerpGuide's Target
        GetSlerpGuide().SetSlerpTargetTransform(itemHeld.DefaultHandSlot.transform);

    }

    public void SetSlerpGuideTarget(Transform newSlerpTarget)
    {

        GetSlerpGuide().SetSlerpTargetTransform(newSlerpTarget);

    }


    /// <summary>
    /// Resets the Slerp Guide's target to the default hand slot.
    /// </summary>
    public void ResetSlerpGuideTarget()
    {
        GetSlerpGuide().SetSlerpTargetTransform(itemHeld.DefaultHandSlot.transform);

        SendMessage("OnResetSlerpGuideTarget", SendMessageOptions.DontRequireReceiver);
    }


}
