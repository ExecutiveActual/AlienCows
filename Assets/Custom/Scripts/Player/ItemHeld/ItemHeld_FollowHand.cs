using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Makes this object smoothly follow a target Transform (position + rotation).
/// </summary>

[Obsolete("Use New SlerpGuideSystem")]
public class ItemHeld_FollowHand : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("The Transform this object will follow.")]
    public Transform TargetHandSlotTransform { get; private set; }

    [Header("Movement Settings")]

    [Tooltip("How many seconds it takes to reach ~63% of the distance (position).")]
    [Range(0.001f, 2f)] public float positionSmoothTime = 0.15f;

    [Tooltip("How many seconds it takes to reach ~63% of the rotation (degrees).")]
    [Range(0.001f, 2f)] public float rotationSmoothTime = 0.12f;

    // Internal velocity for SmoothDamp (do not touch)
    private Vector3 _positionVelocity;


    private void Start()
    {
        if (TargetHandSlotTransform == null)
        {
            TargetHandSlotTransform = GetComponent<ItemHeld>().defaultHandSlot.transform;
        }
    }

    private void Update()
    {
        if (TargetHandSlotTransform == null)
        {
            Debug.LogWarning("ItemHeld_FollowHand: No TargetHandSlotTransform assigned!", this);
            return;
        }

        // ---------- POSITION ----------
        // SmoothDamp gives the nice "inertia → settle" feel.
        Vector3 currentPos = transform.position;
        transform.position = Vector3.SmoothDamp(
            currentPos,
            TargetHandSlotTransform.position,
            ref _positionVelocity,
            positionSmoothTime);

        // ---------- ROTATION ----------
        // Slerp is frame-rate independent when using unscaled time.
        float t = Time.deltaTime / rotationSmoothTime; // approximate lerp factor
        t = Mathf.Clamp01(t); // safety
        transform.rotation = Quaternion.Slerp(transform.rotation, TargetHandSlotTransform.rotation, t);
    }


    public void SetTargetHandSlotTransform(Transform newTarget)
    {
        TargetHandSlotTransform = newTarget;
    }


    // Optional: visual gizmo to see the link in the Scene view
    private void OnDrawGizmosSelected()
    {
        if (TargetHandSlotTransform != null)
        {
            Gizmos.color = new Color(0f, 1f, 0f, 0.75f);
            Gizmos.DrawLine(transform.position, TargetHandSlotTransform.position);
            Gizmos.DrawWireSphere(TargetHandSlotTransform.position, 0.03f);
        }
    }
}