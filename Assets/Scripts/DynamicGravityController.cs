using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class DynamicGravityController : MonoBehaviour
{
    [Header("Base Gravity Settings")]
    [Tooltip("Normal gravity applied when not abducted.")]
    public float normalGravityScale = 1f;

    [Tooltip("Reduced gravity applied during abduction (e.g., 0 = float freely, negative = pull upwards).")]
    public float abductionGravityScale = 0f;

    [Tooltip("How quickly gravity transitions between states.")]
    public float gravityTransitionSpeed = 3f;

    [Header("Upward Pull Settings")]
    [Tooltip("Extra upward force applied during abduction for visual lift effect.")]
    public float abductionLiftForce = 15f;

    [Tooltip("Reference point (like UFO center) that pulls objects upward.")]
    public Transform abductionCenter;

    [Tooltip("Visualize gravity direction and pull in scene view.")]
    public bool showGizmos = true;

    private Rigidbody rb;
    private float currentGravityScale;
    private bool isAbducted = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false; // we'll handle it ourselves
        currentGravityScale = normalGravityScale;
    }

    private void FixedUpdate()
    {
        // Smooth transition between normal and abduction gravity
        float targetScale = isAbducted ? abductionGravityScale : normalGravityScale;
        currentGravityScale = Mathf.Lerp(currentGravityScale, targetScale, Time.fixedDeltaTime * gravityTransitionSpeed);

        // Apply custom gravity
        Vector3 gravity = Physics.gravity * currentGravityScale;
        rb.AddForce(gravity, ForceMode.Acceleration);

        // Apply additional lift if abducted
        if (isAbducted && abductionCenter != null)
        {
            Vector3 liftDir = (abductionCenter.position - transform.position).normalized;
            rb.AddForce(liftDir * abductionLiftForce, ForceMode.Acceleration);
        }
    }

    /// <summary>
    /// Start the abduction effect (weakens gravity and adds lift).
    /// </summary>
    public void StartAbduction(Transform ufoCenter)
    {
        abductionCenter = ufoCenter;
        isAbducted = true;
    }

    /// <summary>
    /// Stop the abduction effect and restore normal gravity.
    /// </summary>
    public void StopAbduction()
    {
        isAbducted = false;
        abductionCenter = null;
    }

    private void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;

        Gizmos.color = isAbducted ? Color.cyan : Color.red;
        if (abductionCenter != null)
        {
            Gizmos.DrawLine(transform.position, abductionCenter.position);
            Gizmos.DrawWireSphere(abductionCenter.position, 0.3f);
        }
    }
}
