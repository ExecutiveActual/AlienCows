using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

/// <summary>
/// UFOController
/// - Finds a single untargeted Cow (tag="Cow") and ensures exclusivity.
/// - Moves toward the cow in a zig-zag trajectory (editable parameters).
/// - Holds fixed Y(the altitude) while moving.
/// - Hands off to UFOAbductor when within abductionRadius.
/// - Returns to spawn point and self-destructs after finishing.
/// - Uses UnityEvents for designer hooks.
/// </summary>
[DisallowMultipleComponent]
public class UFOController : MonoBehaviour
{
    // -----------------------------
    // Inspector fields
    // -----------------------------
    [Header("Targeting")]
    [Tooltip("Tag used by cows (must match scene cows).")]
    public string cowTag = "Cow";

    [Tooltip("Search radius used when acquiring a cow target. If <= 0, searches entire scene.")]
    public float targetSearchRadius = 0f;

    [Header("Movement")]
    [Tooltip("Constant world Y altitude at which UFO will fly.")]
    public float flightAltitude = 20f;

    [Tooltip("Movement speed (units/sec).")]
    public float moveSpeed = 10f;

    [Tooltip("Zig-zag amplitude (horizontal offset in world units).")]
    public float zigZagAmplitude = 4f;

    [Tooltip("Zig-zag frequency (how fast it oscillates).")]
    public float zigZagFrequency = 2f;

    [Tooltip("Rotation speed while moving (degrees/sec).")]
    public float rotationSpeed = 90f;

    [Header("Abduction")]
    [Tooltip("Distance at which the UFO hands off to the abductor.")]
    public float abductionRadius = 6f;

    [Tooltip("If true, the UFO will rotate around its Y-axis while abducting.")]
    public bool rotateWhileAbducting = true;

    [Header("Return & Lifetime")]
    [Tooltip("How long (seconds) the UFO waits at the spawn point before self-destroying.")]
    public float waitAtSpawnBeforeDestroy = 0.5f;

    [Tooltip("If health falls below this fraction (0..1), the UFO flees back to spawn and is considered fled.")]
    [Range(0f, 1f)]
    public float fleeHealthThreshold = 0.3f;

    [Header("Debug")]
    [Tooltip("Enable verbose debug logs for this UFO.")]
    public bool showDebug = false;

    // -----------------------------
    // UnityEvents (Designer hooks)
    // -----------------------------
    [Header("Events")]
    [Tooltip("Invoked when UFO acquires a target cow. Parameter = target Transform.")]
    public UnityEvent<Transform> onTargetAcquired;

    [Tooltip("Invoked when UFO begins approach movement (towards target).")]
    public UnityEvent onApproachStarted;

    [Tooltip("Invoked when UFO hands control to the abductor.")]
    public UnityEvent onHandedToAbductor;

    [Tooltip("Invoked when UFO finishes its lifecycle (destroy or flee).")]
    public UnityEvent onLifecycleEnded;

    [Tooltip("Invoked when UFO begins returning to spawn (flee or after abduction).")]
    public UnityEvent onReturnToSpawn;

    [Tooltip("Invoked when the controlled cow is released (fell or destroyed). Parameter = the cow Transform (may be null if destroyed).")]
    public UnityEvent<Transform> onCowReleased;

    // -----------------------------
    // Runtime state
    // -----------------------------
    [HideInInspector] public Transform spawnPointTransform;
    private Transform currentTargetCow;
    private Vector3 spawnPosition;
    private Quaternion spawnRotation;
    private bool isApproaching = false;
    private bool isReturning = false;
    private bool isAbducting = false;
    private UFOAbductor abductor;
    private UFOHealth health;
    private float approachStartTime;
    private float zigZagSeed;

    // Static set to ensure uniqueness of targets across UFO instances.
    private static HashSet<Transform> globallyTargetedCows = new HashSet<Transform>();

    private void Awake()
    {
        spawnPosition = transform.position;
        spawnRotation = transform.rotation;
        abductor = GetComponent<UFOAbductor>();
        health = GetComponent<UFOHealth>();
        zigZagSeed = Random.Range(0f, 1000f);

        if (health != null)
        {
            // subscribe (UnityEvent style)
            health.onDamageReceived.AddListener(OnDamaged);
            health.onDeath.AddListener(OnDeath);
            health.onHealthPercentChanged.AddListener(OnHealthPercentChanged);
        }

        if (abductor != null)
        {
            abductor.onAbductionStarted.AddListener(OnAbductionStarted);
            abductor.onAbductionStopped.AddListener(OnAbductionStopped);
            abductor.onCowAbducted.AddListener(OnCowAbducted);
        }
    }

    private void OnEnable()
    {
        // ensure UI texts hidden by default; UI script will handle this, but keep safe.
    }

    private void Start()
    {
        // If spawnPointTransform is not set externally, default to initial transform
        if (spawnPointTransform == null)
        {
            spawnPointTransform = new GameObject($"SpawnPoint_{gameObject.name}").transform;
            spawnPointTransform.position = spawnPosition;
            spawnPointTransform.rotation = spawnRotation;
        }
    }

    private void Update()
    {
        // Movement loop: approach or return
        if (isApproaching && currentTargetCow != null)
        {
            ApproachUpdate();
        }
        else if (isReturning)
        {
            ReturnUpdate();
        }
        else if (!isApproaching && !isReturning && !isAbducting)
        {
            // Idle: Try acquire a target if not already assigned.
            TryAcquireTarget();
        }
    }

    // -----------------------------
    // Target selection
    // -----------------------------
    private void TryAcquireTarget()
    {
        if (currentTargetCow != null) return;

        Transform best = null;
        float bestDist = float.MaxValue;
        Vector3 myPos = transform.position;

        GameObject[] cows = GameObject.FindGameObjectsWithTag(cowTag);
        if (cows == null || cows.Length == 0) return;

        foreach (var g in cows)
        {
            Transform t = g.transform;
            if (globallyTargetedCows.Contains(t)) continue;

            if (targetSearchRadius > 0f)
            {
                float d = Vector3.Distance(myPos, t.position);
                if (d > targetSearchRadius) continue;
            }

            float dist = Vector3.Distance(myPos, t.position);
            if (dist < bestDist)
            {
                best = t;
                bestDist = dist;
            }
        }

        if (best != null)
        {
            currentTargetCow = best;
            globallyTargetedCows.Add(currentTargetCow);
            onTargetAcquired?.Invoke(currentTargetCow);
            StartApproach();
            if (showDebug) Debug.Log($"[UFOController] {name} acquired target {currentTargetCow.name}");
        }
    }

    // Called externally (optional) to free a cow early if it died by other system
    public void NotifyCowDestroyed(Transform cow)
    {
        if (cow == null) return;
        globallyTargetedCows.Remove(cow);
        if (cow == currentTargetCow)
        {
            currentTargetCow = null;
            isApproaching = false;
        }
    }

    // -----------------------------
    // Approach movement (zig-zag)
    // -----------------------------
    private void StartApproach()
    {
        if (currentTargetCow == null) return;
        isApproaching = true;
        isReturning = false;
        approachStartTime = Time.time;
        onApproachStarted?.Invoke();
    }

    private void ApproachUpdate()
    {
        if (currentTargetCow == null)
        {
            AbortApproach();
            return;
        }

        Vector3 targetPos = currentTargetCow.position;
        // keep a fixed altitude
        targetPos.y = flightAltitude;

        // compute base direct movement
        Vector3 direction = (targetPos - transform.position);
        Vector3 horizontalDir = new Vector3(direction.x, 0f, direction.z);

        // compute forward move
        Vector3 forwardMove = horizontalDir.normalized * moveSpeed * Time.deltaTime;

        // compute zig-zag offset perpendicular to forward direction
        Vector3 perp = Vector3.Cross(Vector3.up, horizontalDir.normalized);
        float elapsed = Time.time - approachStartTime + zigZagSeed;
        float zig = Mathf.Sin(elapsed * zigZagFrequency) * zigZagAmplitude;

        Vector3 zigOffset = perp * zig * Time.deltaTime; // small offset each frame

        // combine
        Vector3 nextPos = transform.position + forwardMove + zigOffset;
        nextPos.y = flightAltitude; // ensure fixed Y

        // smooth rotation towards movement direction
        Vector3 lookDir = (nextPos - transform.position);
        if (lookDir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(lookDir.normalized, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }

        transform.position = nextPos;

        // check abduction radius (use planar distance)
        float planarDist = Vector3.Distance(new Vector3(transform.position.x, 0f, transform.position.z),
                                           new Vector3(currentTargetCow.position.x, 0f, currentTargetCow.position.z));
        if (planarDist <= abductionRadius)
        {
            // hand control to abductor
            if (abductor != null && !isAbducting)
            {
                isAbducting = true;
                isApproaching = false;
                onHandedToAbductor?.Invoke();
                abductor.BeginAbduction(currentTargetCow, rotateWhileAbducting);
            }
        }
    }

    private void AbortApproach()
    {
        isApproaching = false;
        if (currentTargetCow != null)
            globallyTargetedCows.Remove(currentTargetCow);
        currentTargetCow = null;
    }

    // -----------------------------
    // Return & finish
    // -----------------------------
    private void BeginReturnToSpawn(bool consideredFled)
    {
        isReturning = true;
        isApproaching = false;
        isAbducting = false;
        onReturnToSpawn?.Invoke();
        // if fleeing (low health), tell spawner later that it fled; UFOHealth will also trigger
        StartCoroutine(ReturnRoutine(consideredFled));
    }

    private IEnumerator ReturnRoutine(bool consideredFled)
    {
        // fly back to spawn position at same altitude
        while (Vector3.Distance(new Vector3(transform.position.x, 0f, transform.position.z),
                                new Vector3(spawnPosition.x, 0f, spawnPosition.z)) > 1f)
        {
            Vector3 target = spawnPosition;
            target.y = flightAltitude;
            Vector3 dir = (target - transform.position).normalized;
            transform.position += dir * moveSpeed * Time.deltaTime;

            Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);

            yield return null;
        }

        // we reached spawn
        // If this UFO fled with a cow we should notify spawner via UFOHealth or caller.
        if (showDebug) Debug.Log($"[UFOController] {name} reached spawn and will be destroyed. ConsideredFled={consideredFled}");

        // small wait then destroy
        yield return new WaitForSeconds(waitAtSpawnBeforeDestroy);

        // Report as fled if applicable
        if (consideredFled)
        {
            // Try to report to spawner if exists via SpawnedByUFOSpawner marker
            var marker = GetComponent<MonoBehaviour>()?.GetComponentInChildren<SpawnedByUFOSpawnerMarker>();
            var spawner = GetSpawnerFromMarker();
            if (spawner != null)
            {
                spawner.ReportEnemyFled(UFOHealth.EnemyType.UFO);
            }
        }

        onLifecycleEnded?.Invoke();
        Destroy(gameObject);
    }

    private void ReturnUpdate()
    {
        // handled by coroutine
    }

    // -----------------------------
    // Abductor callbacks
    // -----------------------------
    private void OnAbductionStarted()
    {
        // keep isAbducting true (already set by BeginAbduction)
        if (showDebug) Debug.Log($"[UFOController] {name} started abduction.");
    }

    private void OnAbductionStopped()
    {
        // Cow was released (maybe fell or abducted completed)
        isAbducting = false;
        // free the target if still present
        if (currentTargetCow != null)
        {
            globallyTargetedCows.Remove(currentTargetCow);
            onCowReleased?.Invoke(currentTargetCow);
            currentTargetCow = null;
        }
        // begin return to spawn (not considered fled unless abductor signaled)
        BeginReturnToSpawn(false);
    }

    private void OnCowAbducted()
    {
        // abduction finished successfully: the cow object was destroyed by the abductor.
        isAbducting = false;
        if (currentTargetCow != null)
        {
            globallyTargetedCows.Remove(currentTargetCow); // ensure clean
            currentTargetCow = null;
        }

        // After abducting a cow, UFO returns to spawn (that will be considered a 'fled' for score)
        BeginReturnToSpawn(true);
    }

    // -----------------------------
    // Health callbacks
    // -----------------------------
    private void OnDamaged(float damage)
    {
        // If damaged while abducting, stop abduction and release cow
        if (isAbducting && abductor != null)
        {
            abductor.InterruptAbduction();
            // abductor will call OnAbductionStopped which will begin return
        }
    }

    private void OnDeath()
    {
        // On death: replace mesh/effects handled by UFOHealth; now tell spawner about destroyed
        var spawner = GetSpawnerFromMarker();
        if (spawner != null)
        {
            spawner.ReportEnemyDestroyed(UFOHealth.EnemyType.UFO);
        }

        // cleanup targeted cow if any
        if (currentTargetCow != null)
        {
            globallyTargetedCows.Remove(currentTargetCow);
            currentTargetCow = null;
        }

        onLifecycleEnded?.Invoke();
        // Destroy will be handled by UFOHealth (it can swap model then destroy); ensure this object destroyed eventually
    }

    private void OnHealthPercentChanged(float percent)
    {
        // If below flee threshold, begin return/flee behavior
        if (percent <= fleeHealthThreshold && !isReturning)
        {
            // interrupt abduction if in progress
            if (isAbducting && abductor != null) abductor.InterruptAbduction();

            // mark as fleeing and begin return considering it fled (the abductor may have had a cow)
            BeginReturnToSpawn(true);
        }
    }

    // -----------------------------
    // Utilities
    // -----------------------------
    private UFOSpawner GetSpawnerFromMarker()
    {
        // Try to find the helper marker attached by your spawner (class names may differ)
        var marker = GetComponent<SpawnedByUFOSpawnerMarker>();
        if (marker != null) return marker.spawner;
        // fallback: search children
        var childMarker = GetComponentInChildren<SpawnedByUFOSpawnerMarker>();
        if (childMarker != null) return childMarker.spawner;
        return null;
    }

    // Public: allow forced release of cow if other systems demand it
    public void ForceReleaseCurrentCow()
    {
        if (isAbducting && abductor != null)
        {
            abductor.InterruptAbduction();
        }
        else if (currentTargetCow != null)
        {
            globallyTargetedCows.Remove(currentTargetCow);
            currentTargetCow = null;
        }
    }
}

/// <summary>
/// Small marker definition to allow cross-script referencing of spawner.
/// Keep this class name consistent with your spawner marker if using a different one.
/// </summary>
public class SpawnedByUFOSpawnerMarker : MonoBehaviour
{
    public UFOSpawner spawner;
}
