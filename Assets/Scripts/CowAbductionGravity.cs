using UnityEngine;

/// <summary>
/// CowAbductionGravity
/// Attach this to any cow (or abductable object) with a Rigidbody.
/// It handles:
/// - Custom gravity scaling
/// - Upward pull during abduction
/// - Auto-drop when the UFO is destroyed or reference is lost
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class CowAbductionGravity : MonoBehaviour
{
    [Header("Gravity Settings")]
    [Tooltip("Normal gravity scale when cow is not abducted (1 = default Unity gravity).")]
    public float normalGravityScale = 1f;

    [Tooltip("Gravity scale while abducted (0 = floaty, negative = upward gravity).")]
    public float abductedGravityScale = 0.1f;

    [Tooltip("How fast gravity scale interpolates when state changes.")]
    public float gravityLerpSpeed = 5f;

    [Header("Abduction Lift")]
    [Tooltip("Extra upward (or towards UFO) acceleration while abducted.")]
    public float upwardLiftForce = 15f;

    [Tooltip("Clamp for maximum upward speed so cows don't yeet into space.")]
    public float maxUpwardSpeed = 12f;

    [Header("Debug")]
    [SerializeField] private bool isAbducted = false;

    private Rigidbody rb;

    // The UFO that abducted this cow (optional, but useful to detect destruction)
    private Transform ufoTransform;
    private HealthManager ufoHealth; // optional reference if your UFO uses HealthManager

    private float currentGravityScale;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // We'll handle gravity ourselves to allow custom scaling.
        rb.useGravity = false;
        currentGravityScale = normalGravityScale;
    }

    private void FixedUpdate()
    {
        // If abducted, continuously check if the UFO is gone or dead.
        if (isAbducted)
        {
            bool ufoGone = (ufoTransform == null);
            bool ufoDead = (ufoHealth != null && ufoHealth.isDead);

            if (ufoGone || ufoDead)
            {
                // UFO disappeared or died -> auto drop
                StopAbduction();
            }
        }

        // Decide target gravity scale based on state
        float targetScale = isAbducted ? abductedGravityScale : normalGravityScale;
        currentGravityScale = Mathf.Lerp(currentGravityScale, targetScale, Time.fixedDeltaTime * gravityLerpSpeed);

        // Apply custom gravity
        Vector3 gravityForce = Physics.gravity * currentGravityScale;
        rb.AddForce(gravityForce, ForceMode.Acceleration);

        // Apply extra lift while abducted
        if (isAbducted)
        {
            Vector3 liftDir = Vector3.up;

            // If we do have a UFO transform, we can pull roughly towards it
            if (ufoTransform != null)
            {
                Vector3 toUfo = (ufoTransform.position - transform.position).normalized;
                // Blend between straight up and towards UFO (optional)
                liftDir = Vector3.Lerp(Vector3.up, toUfo, 0.5f).normalized;
            }

            rb.AddForce(liftDir * upwardLiftForce, ForceMode.Acceleration);

            // Clamp upward velocity
            if (rb.linearVelocity.y > maxUpwardSpeed)
            {
                Vector3 v = rb.linearVelocity;
                v.y = maxUpwardSpeed;
                rb.linearVelocity = v;
            }
        }
    }

    /// <summary>
    /// Called by UFO when abduction starts.
    /// You MUST call this once to mark the cow as abducted.
    /// </summary>
    public void BeginAbduction(Transform ufo, HealthManager ufoHealthManager = null)
    {
        ufoTransform = ufo;
        ufoHealth = ufoHealthManager;
        isAbducted = true;
    }

    /// <summary>
    /// Stops abduction and restores normal gravity.
    /// This is also called automatically if the UFO disappears.
    /// </summary>
    public void StopAbduction()
    {
        isAbducted = false;
        ufoTransform = null;
        ufoHealth = null;
    }

    /// <summary>
    /// Force-immediately drop the cow (for external scripts if needed).
    /// </summary>
    public void ForceDrop()
    {
        StopAbduction();
        // can tweak velocity here if you want extra impact
    }

    private void OnDisable()
    {
        // In case object gets disabled, reset state cleanly.
        isAbducted = false;
        ufoTransform = null;
        ufoHealth = null;
    }
}
