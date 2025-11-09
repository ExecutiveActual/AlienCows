using UnityEngine;
using UnityEngine.Events;

public class ItemHeld_SlerpGuide_Manager : MonoBehaviour
{
    
    private ItemHeld itemHeld;

    private DumbSlerpToTarget this_DumbSlerpToTarget;


    [SerializeField] private DumbSlerpToTarget _SlerpGuide;

    public DumbSlerpToTarget SlerpGuide { get; private set; }


    [Header("Slerp Guide Movement Settings")]

    [Tooltip("How many seconds it takes to reach ~63% of the distance (position).")]
    [Range(0.001f, 2f)] public float positionSmoothTime_Guide = 0.012f;

    [Tooltip("How many seconds it takes to reach ~63% of the rotation (degrees).")]
    [Range(0.001f, 2f)] public float rotationSmoothTime_Guide = 0.052f;


    //public UnityEvent UE_OnSlerpGuideTargetChanged;

    public UnityEvent UE_OnResetSlerpGuideTarget;



    private void Awake()
    {
        itemHeld = GetComponent<ItemHeld>();

        this_DumbSlerpToTarget = GetComponent<DumbSlerpToTarget>();

        SlerpGuide = _SlerpGuide;

        if (SlerpGuide == null)
        {
            GameObject go = new GameObject("SlerpGuide_" + itemHeld.ItemName);
            SlerpGuide = go.AddComponent<DumbSlerpToTarget>();
            SlerpGuide.positionSmoothTime = positionSmoothTime_Guide;
            SlerpGuide.rotationSmoothTime = rotationSmoothTime_Guide;
        }
    }


    private void OnDestroy()
    {
        if (SlerpGuide != null)
            Destroy(SlerpGuide.gameObject);
    }



    private void Start()
    {

        if (SlerpGuide != null)
            SlerpGuide.transform.SetParent(null);

        Debug.Log(SlerpGuide);

        // Set This SlerpTarget
        this_DumbSlerpToTarget.SetSlerpTargetTransform(SlerpGuide.transform);

        // Set SlerpGuide's Target
        SlerpGuide.SetSlerpTargetTransform(itemHeld.DefaultHandSlot.transform);

    }

    public void SetSlerpGuideTarget(Transform newSlerpTarget)
    {

        SlerpGuide.SetSlerpTargetTransform(newSlerpTarget);

    }


    /// <summary>
    /// Resets the Slerp Guide's target to the default hand slot.
    /// </summary>
    public void ResetSlerpGuideTarget()
    {
        UE_OnResetSlerpGuideTarget?.Invoke();

        SlerpGuide.SetSlerpTargetTransform(itemHeld.DefaultHandSlot.transform);

    }


}
