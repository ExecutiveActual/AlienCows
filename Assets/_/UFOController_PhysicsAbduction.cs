using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// UFOController_PhysicsAbduction
/// Handles movement, abduction (via CowAbductionGravity), and realistic physics drops.
/// </summary>
[RequireComponent(typeof(HealthManager))]
public class UFOController_PhysicsAbduction : MonoBehaviour
{
    [Header("General Settings")]
    public string cowTag = "Cow";
    public float yHeight = 12f;
    public float searchInterval = 1f;
    public float maxSearchDistance = 60f;

    [Header("Movement Settings")]
    public float moveSpeed = 6f;
    public float returnSpeed = 6f;

    [Header("Zig-Zag Trajectory")]
    public float zigzagAmplitude = 2f;
    public float zigzagFrequency = 2f;
    public float forwardFollowOffset = 0f;
    public float zigzagPhase = 0f;

    [Header("Abduction Settings")]
    public float detectionRadius = 3.5f;
    public float abductDuration = 8f;
    public float liftForceMin = 10f;
    public float liftForceMax = 25f;
    public float abductGravityMin = 0.3f;
    public float abductGravityMax = -0.2f;

    [Header("FX References")]
    public ParticleSystem abductFX;

    private enum UFOState { Searching, Chasing, Abducting, Returning }
    private UFOState currentState = UFOState.Searching;

    private Transform currentTarget;
    private Vector3 originPosition;
    private Quaternion originRotation;
    private float searchTimer = 0f;
    private float localPhase = 0f;

    private Coroutine abductRoutine;
    private HealthManager healthManager;

    private static readonly HashSet<int> claimedTargets = new HashSet<int>();
    private static readonly object claimLock = new object();

    // ---------------- INITIALIZATION ----------------
    private void Awake()
    {
        healthManager = GetComponent<HealthManager>();
        healthManager.UE_OnDeath.AddListener(OnUFODestroyed);
    }

    private void Start()
    {
        originPosition = transform.position;
        originRotation = transform.rotation;

        Vector3 pos = transform.position;
        pos.y = yHeight;
        transform.position = pos;

        localPhase = zigzagPhase + Random.Range(0f, Mathf.PI * 2f);
        currentState = UFOState.Searching;

        if (abductFX != null)
            abductFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    private void Update()
    {
        // Maintain constant flight height
        Vector3 pos = transform.position;
        pos.y = yHeight;
        transform.position = pos;

        switch (currentState)
        {
            case UFOState.Searching:
                HandleSearching();
                break;
            case UFOState.Chasing:
                HandleChasing();
                break;
            case UFOState.Abducting:
                // handled by coroutine
                break;
            case UFOState.Returning:
                HandleReturning();
                break;
        }
    }

    // ---------------- SEARCHING ----------------
    private void HandleSearching()
    {
        searchTimer -= Time.deltaTime;
        if (searchTimer <= 0f)
        {
            searchTimer = searchInterval;
            FindTarget();
        }
    }

    private void FindTarget()
    {
        GameObject[] cows = GameObject.FindGameObjectsWithTag(cowTag);
        Transform nearest = null;
        float minDist = float.MaxValue;

        foreach (GameObject cow in cows)
        {
            if (cow == null) continue;
            int id = cow.GetInstanceID();

            lock (claimLock)
            {
                if (claimedTargets.Contains(id))
                    continue;
            }

            float dist = Vector3.Distance(transform.position, cow.transform.position);
            if (dist < minDist && dist < maxSearchDistance)
            {
                minDist = dist;
                nearest = cow.transform;
            }
        }

        if (nearest != null)
        {
            lock (claimLock)
            {
                int id = nearest.gameObject.GetInstanceID();
                if (!claimedTargets.Contains(id))
                    claimedTargets.Add(id);
            }

            currentTarget = nearest;
            currentState = UFOState.Chasing;
        }
    }

    // ---------------- CHASING ----------------
    private void HandleChasing()
    {
        if (currentTarget == null)
        {
            ReleaseClaim();
            currentState = UFOState.Searching;
            return;
        }

        MoveZigZagTowards(currentTarget.position, moveSpeed);

        Vector3 ufoXZ = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 cowXZ = new Vector3(currentTarget.position.x, 0, currentTarget.position.z);

        if (Vector3.Distance(ufoXZ, cowXZ) <= detectionRadius)
        {
            if (abductRoutine == null)
                abductRoutine = StartCoroutine(AbductCow(currentTarget));
        }
    }

    private void MoveZigZagTowards(Vector3 target, float speed)
    {
        Vector3 dir = (new Vector3(target.x, 0, target.z) - new Vector3(transform.position.x, 0, transform.position.z)).normalized;
        Vector3 perp = Vector3.Cross(dir, Vector3.up).normalized;

        float oscillation = Mathf.Sin((Time.time + localPhase) * zigzagFrequency) * zigzagAmplitude;
        Vector3 approach = new Vector3(target.x, yHeight, target.z) + perp * oscillation + dir * forwardFollowOffset;

        transform.position = Vector3.MoveTowards(transform.position, approach, speed * Time.deltaTime);

        if ((approach - transform.position).sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation((approach - transform.position).normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 4f);
        }
    }

    // ---------------- ABDUCTION ----------------
    private IEnumerator AbductCow(Transform cow)
    {
        currentState = UFOState.Abducting;

        if (abductFX != null)
            abductFX.Play(true);

        if (cow == null)
        {
            ReleaseClaim();
            currentState = UFOState.Searching;
            yield break;
        }

        CowAbductionGravity cowGrav = cow.GetComponent<CowAbductionGravity>();
        if (cowGrav != null)
        {
            cowGrav.BeginAbduction(transform, healthManager);
        }

        float timer = 0f;
        while (timer < abductDuration)
        {
            if (healthManager.isDead)
            {
                StopFX();
                DropCow(cow);
                yield break;
            }

            // Optional: make beam stronger over time (for visuals)
            if (cowGrav != null)
            {
                float t = timer / abductDuration;
                cowGrav.upwardLiftForce = Mathf.Lerp(liftForceMin, liftForceMax, t);
                cowGrav.abductedGravityScale = Mathf.Lerp(abductGravityMin, abductGravityMax, t);
            }

            timer += Time.deltaTime;
            yield return null;
        }

        // Finished abduction → remove cow
        if (cow != null)
            Destroy(cow.gameObject);

        StopFX();
        ReleaseClaim();
        currentTarget = null;
        abductRoutine = null;
        currentState = UFOState.Returning;
    }

    // ---------------- DROP ----------------
    private void DropCow(Transform cow)
    {
        if (cow == null) return;

        cow.SetParent(null);

        CowAbductionGravity cowGrav = cow.GetComponent<CowAbductionGravity>();
        if (cowGrav != null)
            cowGrav.ForceDrop();

        ReleaseClaim();
        abductRoutine = null;
    }

    // ---------------- RETURNING ----------------
    private void HandleReturning()
    {
        MoveZigZagTowards(originPosition, returnSpeed);

        Vector3 ufoXZ = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 originXZ = new Vector3(originPosition.x, 0, originPosition.z);

        if (Vector3.Distance(ufoXZ, originXZ) < 0.3f)
        {
            transform.position = new Vector3(originPosition.x, yHeight, originPosition.z);
            transform.rotation = originRotation;
            currentState = UFOState.Searching;
        }
    }

    // ---------------- DESTRUCTION ----------------
    private void OnUFODestroyed()
    {
        // Drop current cow if mid-abduction
        if (currentState == UFOState.Abducting && currentTarget != null)
            DropCow(currentTarget);

        StopFX();
        ReleaseClaim();
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        ReleaseClaim();
        StopFX();
    }

    private void StopFX()
    {
        if (abductFX != null && abductFX.isPlaying)
            abductFX.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }

    private void ReleaseClaim()
    {
        if (currentTarget == null) return;
        int id = currentTarget.gameObject.GetInstanceID();
        lock (claimLock)
        {
            claimedTargets.Remove(id);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.2f, 0.9f, 1f, 0.4f);
        Vector3 center = transform.position;
        center.y = yHeight;
        Gizmos.DrawWireSphere(center, detectionRadius);
    }
}
