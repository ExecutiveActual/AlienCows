using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(HealthManager))]
public class UFOController_ : MonoBehaviour
{
    [Header("General Settings")]
    public string cowTag = "Cow";
    public float yHeight = 12f;
    public float searchInterval = 1f;
    public float maxSearchDistance = 60f;

    [Header("Movement Settings")]
    public float moveSpeed = 6f;
    public float returnSpeed = 6f;

    [Header("Spawn and Despawn Movement")]
    public float outOfBounds_YOffset = 300f;
    public float outOfBounds_MoveSpeed = 10f;

    [Header("Zig-Zag Trajectory")]
    public float zigzagAmplitude = 2f;
    public float zigzagFrequency = 2f;
    public float forwardFollowOffset = 0f;
    public float zigzagPhase = 0f;

    [Header("Abduction Settings")]
    public float detectionRadius = 3.5f;
    public float liftSpeed = 4f;
    public float abductDuration = 10f;
    public Vector3 cowAttachOffset = new Vector3(0f, -0.5f, 0f);

    [Header("Object References")]
    public ParticleSystem abductFX;
    public Transform meshObject;


    private enum UFOState { Searching, Chasing, Abducting, Returning, Despawning }
    private UFOState currentState = UFOState.Searching;

    private Transform currentTarget;
    private Vector3 originPosition;
    private Quaternion originRotation;
    private float searchTimer = 0f;
    private float localPhase = 0f;

    private Coroutine abductRoutine;
    private HealthManager healthManager;


    //Despawn Variables
    private Coroutine despawnCoroutine;
    private Vector3 despawnStartPos;
    private Vector3 despawnTargetPos;
    private Vector3 meshOriginalScale;
    private float despawnTotalDistance;
    private float despawnTravelled = 0f;
    private bool isDespawning = false;


    private Cow_Abduction cowAbduction_Curr;

    private static readonly HashSet<int> claimedTargets = new HashSet<int>();
    private static readonly object claimLock = new object();

    // ---------------- UNITY ----------------
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
        {
            abductFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    private void Update()
    {

        if (currentState != UFOState.Despawning)
        {
            // Keep UFO fixed at flight height
            Vector3 pos = transform.position;
            pos.y = yHeight;
            transform.position = pos;
        }
        

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
            case UFOState.Despawning:
                HandleDespawning();
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

        // Move above the cow's XZ position
        Vector3 targetPos = new Vector3(currentTarget.position.x, yHeight, currentTarget.position.z);
        transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);

        // Rotate only on Y axis (stay level)
        Vector3 toCow = currentTarget.position - transform.position;
        toCow.y = 0f;
        if (toCow.sqrMagnitude > 0.001f)
        {
            Quaternion lookRot = Quaternion.LookRotation(toCow.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 2f);
        }

        // Begin abduction only when perfectly above cow
        float distanceXZ = Vector3.Distance(
            new Vector3(transform.position.x, 0, transform.position.z),
            new Vector3(currentTarget.position.x, 0, currentTarget.position.z)
        );

        if (distanceXZ < 0.5f)
        {
            if (abductRoutine == null)
                abductRoutine = StartCoroutine(AbductCow(currentTarget));
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

        // Lock directly above cow
        Vector3 hoverPos = new Vector3(cow.position.x, yHeight, cow.position.z);
        while (Vector3.Distance(transform.position, hoverPos) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, hoverPos, moveSpeed * Time.deltaTime);
            yield return null;
        }


        // Nick
        if (cow != null)
        {
            cowAbduction_Curr = cow.GetComponent<Cow_Abduction>();

            cowAbduction_Curr.StartAbduction();
        }


        // Lift cow
        Vector3 attachPoint = transform.position + cowAttachOffset;
        while (cow != null && Vector3.Distance(cow.position, attachPoint) > 0.05f)
        {

            // THIS CODE DOES NOT RUN !!!
            if (healthManager.isDead)
            {
                StopFX();
                cow.SetParent(null);
                Debug.Log("UFO destroyed during abduction, releasing cow.");
                yield break;
            }

            // THIS WORKS:
            cow.position = Vector3.MoveTowards(cow.position, attachPoint, liftSpeed * Time.deltaTime);
            if(!cowAbduction_Curr.GetIsAirborne()) cowAbduction_Curr.SetAirborneTrue();
            yield return null;
        }

        if (cow != null)
        {
            cow.SetParent(transform);
            cow.localPosition = cowAttachOffset;
        }

        StopFX();

        float timer = 0f;
        while (timer < abductDuration)
        {
            if (healthManager.isDead)
            {
                StopFX();
                cow.SetParent(null);
                yield break;
            }

            transform.position = hoverPos;
            timer += Time.deltaTime;
            yield return null;
        }

        if (cow != null)
            Destroy(cow.gameObject);

        StopFX();
        ReleaseClaim();
        currentTarget = null;
        abductRoutine = null;
        currentState = UFOState.Returning;
    }

    // ---------------- RETURNING ----------------
    private void HandleReturning()
    {
        //MoveZigZagTowards(originPosition, returnSpeed);

        transform.position = Vector3.MoveTowards(transform.position, originPosition, returnSpeed * Time.deltaTime);

        Vector3 ufoXZ = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 originXZ = new Vector3(originPosition.x, 0, originPosition.z);
        if (Vector3.Distance(ufoXZ, originXZ) < 1f)
        {
            transform.position = new Vector3(originPosition.x, yHeight, originPosition.z);
            transform.rotation = originRotation;
            currentState = UFOState.Despawning;

            // Important: reset so HandleDespawning() knows to init on first frame
            isDespawning = false;

            Debug.Log("Done with Return");
        }
    }

    // ---------------- DESPAWNING ----------------
    // REPLACE your entire HandleDespawning() method with this one
    private void HandleDespawning()
    {
        if (!isDespawning)
        {
            // First frame setup
            isDespawning = true;
            despawnTravelled = 0f;

            healthManager.SetInvincible(true);

            despawnStartPos = transform.position;
            despawnTargetPos = GetYOffsetPosition(originPosition, outOfBounds_YOffset);
            despawnTotalDistance = Vector3.Distance(despawnStartPos, despawnTargetPos);
            meshOriginalScale = meshObject.localScale;

            if (despawnTotalDistance < 0.01f)
            {
                FinishDespawn();
                return;
            }
        }

        // Advance progress
        despawnTravelled += outOfBounds_MoveSpeed * Time.deltaTime;
        float t = Mathf.Clamp01(despawnTravelled / despawnTotalDistance);

        // Ease-In Quadratic → starts slow, then WHOOSH upward!
        float easedT = t * t;

        // Smooth position with acceleration
        transform.position = Vector3.Lerp(despawnStartPos, despawnTargetPos, easedT);

        // Perfectly synced scale down
        meshObject.localScale = Vector3.Lerp(meshOriginalScale, Vector3.zero, easedT);

        // Done?
        if (t >= 1f)
        {
            FinishDespawn();
        }
    }

    // ADD this helper method (anywhere in the class)
    private void FinishDespawn()
    {
        isDespawning = false;
        transform.position = despawnTargetPos;
        meshObject.localScale = Vector3.zero;

        cowAbduction_Curr = null;
        Debug.LogWarning("DESPAWN IS HAPPENING");
        GameManager_Singleton.Instance.GetComponent<GameManager_SaveSystem>().PlayerData_Curr.CowAmount -= 1;
        Destroy(gameObject);
    }


    private Vector3 GetYOffsetPosition(Vector3 initialPos, float yOffset)
    {

        return new Vector3(initialPos.x, initialPos.y + yOffset, initialPos.z);

    }


    // ---------------- UTILITIES ----------------
    private void MoveZigZagTowards(Vector3 target, float speed)
    {
        Vector3 dir = (new Vector3(target.x, 0, target.z) - new Vector3(transform.position.x, 0, transform.position.z)).normalized;
        Vector3 perp = Vector3.Cross(dir, Vector3.up).normalized;

        float oscillation = Mathf.Sin((Time.time + localPhase) * zigzagFrequency) * zigzagAmplitude;
        Vector3 approach = new Vector3(target.x, yHeight, target.z) + perp * oscillation + dir * forwardFollowOffset;

        transform.position = Vector3.MoveTowards(transform.position, approach, speed * Time.deltaTime);

        // Rotate only around Y (stay level)
        if ((approach - transform.position).sqrMagnitude > 0.001f)
        {
            Vector3 flatDir = approach - transform.position;
            flatDir.y = 0f;
            if (flatDir.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(flatDir.normalized, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 4f);
            }
        }
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

    private void OnUFODestroyed()
    {
        StopFX();
        ReleaseClaim();
        StartCoroutine(DestroyAfterFrame());
    }

    private IEnumerator DestroyAfterFrame()
    {
        yield return null;
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (cowAbduction_Curr != null)
        {
            cowAbduction_Curr.transform.parent = null;
            cowAbduction_Curr.StopAbduction();

            cowAbduction_Curr = null;
        }

        ReleaseClaim();
        StopFX();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.2f, 0.9f, 1f, 0.4f);
        Vector3 center = transform.position;
        center.y = yHeight;
        Gizmos.DrawWireSphere(center, detectionRadius);
    }
}
