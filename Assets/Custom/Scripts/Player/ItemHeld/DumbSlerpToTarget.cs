using UnityEngine;

public class DumbSlerpToTarget : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("The Transform this object will follow.")]
    public Transform SlerpTargetTransform { get; private set; }

    [SerializeField] private Transform _SlerpTargetTransform;

    [Header("Movement Settings")]

    [Tooltip("How many seconds it takes to reach ~63% of the distance (position).")]
    [Range(0.001f, 2f)] public float positionSmoothTime = 0.15f;

    [Tooltip("How many seconds it takes to reach ~63% of the rotation (degrees).")]
    [Range(0.001f, 2f)] public float rotationSmoothTime = 0.12f;

    // Internal velocity for SmoothDamp (do not touch)
    private Vector3 _positionVelocity;


    private void Awake()
    {
        SlerpTargetTransform = _SlerpTargetTransform;
    }

    private void Start()
    {
        if (SlerpTargetTransform == null)
        {
            Debug.LogWarning("DumbSlerpToTarget: No SlerpTargetTransform assigned at Start!", this);
        }
    }

    private void Update()
    {
        if (SlerpTargetTransform == null)
        {
            //Debug.LogWarning("DumbSlerpToTarget: No SlerpTargetTransform assigned!", this);
            return;
        }

        // ---------- POSITION ----------
        // SmoothDamp gives the nice "inertia → settle" feel.
        Vector3 currentPos = transform.position;
        transform.position = Vector3.SmoothDamp(
            currentPos,
            SlerpTargetTransform.position,
            ref _positionVelocity,
            positionSmoothTime);

        // ---------- ROTATION ----------
        // Slerp is frame-rate independent when using unscaled time.
        float t = Time.deltaTime / rotationSmoothTime; // approximate lerp factor
        t = Mathf.Clamp01(t); // safety
        transform.rotation = Quaternion.Slerp(transform.rotation, SlerpTargetTransform.rotation, t);
    }


    public void SetSlerpTargetTransform(Transform newTarget)
    {
        SlerpTargetTransform = newTarget;
    }


    // Optional: visual gizmo to see the link in the Scene view
    private void OnDrawGizmosSelected()
    {
        if (SlerpTargetTransform != null)
        {
            Gizmos.color = new Color(0f, 1f, 0f, 0.75f);
            Gizmos.DrawLine(transform.position, SlerpTargetTransform.position);
            Gizmos.DrawWireSphere(SlerpTargetTransform.position, 0.03f);
        }
    }
}
