using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

/// <summary>
/// UFOAbductor
/// - Controls abduction beam, lifting motion, optional rotation during abduction.
/// - Destroys the cow after reaching liftHeight (simulates successful abduction).
/// - Fires UnityEvents: onAbductionStarted, onAbductionStopped, onCowAbducted.
/// - If interrupted via InterruptAbduction(), cow is released and falls (if Rigidbody present).
/// </summary>
[DisallowMultipleComponent]
public class UFOAbductor : MonoBehaviour
{
    [Header("Beam & Visuals")]
    [Tooltip("Transform where the beam originates (usually under the UFO).")]
    public Transform beamOrigin;

    [Tooltip("Beam GameObject (enable/disable to show beam).")]
    public GameObject beamObject;

    [Header("Abduction Parameters")]
    [Tooltip("Height offset above ground that counts as 'abducted' (World Y).")]
    public float liftTargetHeight = 30f;

    [Tooltip("Speed (units/sec) at which cow is lifted.")]
    public float liftSpeed = 4f;

    [Tooltip("If true, UFO rotates around Y axis while abducting.")]
    public bool defaultRotateWhileAbducting = true;

    [Header("Rotation (while abducting)")]
    [Tooltip("Degrees per second for optional rotation while abducting.")]
    public float abductRotationSpeed = 90f;

    [Header("Audio")]
    [Tooltip("Audio source to play beam/abduction sounds.")]
    public AudioSource beamAudioSource;

    [Tooltip("Audio clip to play when abduction starts.")]
    public AudioClip abductionStartClip;

    [Tooltip("Volume for abduction start clip.")]
    [Range(0f, 1f)]
    public float abductionStartVolume = 1f;

    [Header("Events")]
    public UnityEvent onAbductionStarted;
    public UnityEvent onAbductionStopped;
    public UnityEvent onCowAbducted;
    public UnityEvent<GameObject> onCowReleased; // param = cow GO (if available)

    private Coroutine liftRoutine;
    private Transform currentCow;
    private bool isAbducting = false;
    private bool rotationEnabled = false;

    private void Reset()
    {
        // safe defaults
        beamObject = beamObject;
    }

    /// <summary>
    /// Called by controller to begin abduction; returns immediately and handles coroutine.
    /// </summary>
    public void BeginAbduction(Transform cow, bool rotateDuring)
    {
        if (cow == null) return;
        if (isAbducting) return;

        currentCow = cow;
        rotationEnabled = rotateDuring;
        isAbducting = true;
        if (beamObject != null) beamObject.SetActive(true);

        if (beamAudioSource != null && abductionStartClip != null)
        {
            beamAudioSource.PlayOneShot(abductionStartClip, abductionStartVolume);
        }

        onAbductionStarted?.Invoke();
        liftRoutine = StartCoroutine(LiftCowRoutine());
    }

    /// <summary>
    /// Interrupt abduction (called by health/damage or external systems). Releases cow.
    /// </summary>
    public void InterruptAbduction()
    {
        if (!isAbducting) return;
        // stop lift coroutine & release cow
        if (liftRoutine != null) StopCoroutine(liftRoutine);
        ReleaseCow(isDestroyed: false);
    }

    private IEnumerator LiftCowRoutine()
    {
        // If the cow has a rigidbody, make it kinematic while lifting; else we parent to beam.
        Rigidbody cowRb = (currentCow != null) ? currentCow.GetComponent<Rigidbody>() : null;
        Transform cowTransform = currentCow;

        if (cowTransform == null)
        {
            // nothing to do
            EndAbductionNoCow();
            yield break;
        }

        // Prepare cow
        if (cowRb != null)
        {
            cowRb.isKinematic = true;
        }

        // parent cow to beam origin (maintains local offsets)
        Vector3 originalLocalPosition = Vector3.zero;
        Transform originalParent = cowTransform.parent;
        cowTransform.SetParent(beamOrigin != null ? beamOrigin : transform, worldPositionStays: true);

        // lift until reaching target lift height
        while (cowTransform != null && cowTransform.position.y < liftTargetHeight)
        {
            float step = liftSpeed * Time.deltaTime;
            cowTransform.position = Vector3.MoveTowards(cowTransform.position, new Vector3(cowTransform.position.x, liftTargetHeight, cowTransform.position.z), step);

            // rotate UFO (optional)
            if (rotationEnabled)
            {
                transform.Rotate(Vector3.up, abductRotationSpeed * Time.deltaTime, Space.Self);
            }

            yield return null;
        }

        // At this point cow reached lift height -> simulate successful abduction
        // Destroy/notify cow
        if (cowTransform != null)
        {
            GameObject cowGO = cowTransform.gameObject;
            // detach to avoid destroying parented stuff incorrectly
            cowTransform.SetParent(originalParent, worldPositionStays: true);

            // If cow has custom death logic, better if cow is told to die; else we destroy
            Destroy(cowGO);
        }

        // complete abduction
        isAbducting = false;
        if (beamObject != null) beamObject.SetActive(false);
        onCowAbducted?.Invoke();

        yield break;
    }

    private void EndAbductionNoCow()
    {
        if (beamObject != null) beamObject.SetActive(false);
        isAbducting = false;
        onAbductionStopped?.Invoke();
    }

    private void ReleaseCow(bool isDestroyed)
    {
        // cow falls: if rigidbody exists, enable physics; else just unparent
        if (currentCow != null)
        {
            var rb = currentCow.GetComponent<Rigidbody>();
            Transform prevParent = currentCow.parent;
            currentCow.SetParent(null, worldPositionStays: true);
            if (rb != null)
            {
                rb.isKinematic = false;
                // give a small downward impulse to simulate falling
                rb.AddForce(Vector3.down * 2f, ForceMode.Impulse);
            }

            onCowReleased?.Invoke(currentCow.gameObject);
        }

        if (beamObject != null) beamObject.SetActive(false);
        isAbducting = false;
        onAbductionStopped?.Invoke();
        currentCow = null;
    }

    /// <summary>
    /// Called by external systems to signal abduction finished (if the abductor destroyed the cow itself).
    /// </summary>
    public void NotifyCowDestroyedExternally()
    {
        // ensure we stop everything
        if (liftRoutine != null) StopCoroutine(liftRoutine);
        isAbducting = false;
        if (beamObject != null) beamObject.SetActive(false);
        onCowAbducted?.Invoke();
    }
}
