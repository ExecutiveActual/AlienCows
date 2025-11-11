using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class UFO_ : MonoBehaviour
{
    private enum UfoState { Approach, Abducting, Returning }

    [Header("References")]
    public WorldClock_ worldClock;
    public UFOSpawner_ spawnerRef;
    [Tooltip("Tag used to find cow targets.")]
    public string targetTag = "Cow";

    [Header("Movement")]
    public float moveSpeed = 10f;
    public float rotationSpeed = 8f;
    [Tooltip("Fixed flight height for UFOs.")]
    public float flyHeight = 25f;
    [Tooltip("Distance from cow before abduction begins.")]
    public float abductRangeXZ = 3f;

    [Header("Abduction Settings")]
    public float hoverBeforeLiftTime = 1f;
    public float cowLiftDuration = 10f;
    public float postAbductionDelay = 1f;

    [Header("Lighting")]
    public Light spotLight;
    public float abductionLightIntensity = 8f;
    public Color abductionCompleteColor = Color.cyan;
    public int blinkCount = 3;
    public float blinkSpeed = 0.25f;

    [Header("Sine Flight")]
    public float sineAmplitude = 6f;
    public float sineFrequency = 0.5f;

    [Header("Fall Settings")]
    public float fallDelay = 2f;

    // ───────── Runtime
    private static readonly HashSet<Transform> takenTargets = new HashSet<Transform>();
    private Transform target;
    private Vector3 spawnPoint;
    private float sineTravel;
    private float abductTimer;
    private bool cowDestroyed;
    private Transform liftingCow;
    private Coroutine abductionRoutine;
    private UfoState state = UfoState.Approach;

    //──────────────────────────────────────────────

    void Awake()
    {
        if (!worldClock) worldClock = FindObjectOfType<WorldClock_>();
        if (!spotLight)  spotLight  = GetComponentInChildren<Light>(true);
    }

    void OnEnable()
    {
        spawnPoint = transform.position;
        AcquireUniqueTarget();
        cowDestroyed = false;
        sineTravel = abductTimer = 0f;
        if (spotLight) spotLight.intensity = 0f;
    }

    void Update()
    {
        if (!Application.isPlaying)
        {
            var p = transform.position; p.y = flyHeight; transform.position = p;
            return;
        }

        if (!worldClock) return;

        if (worldClock.isDay)
        {
            if (abductionRoutine != null) StopCoroutine(abductionRoutine);
            state = UfoState.Returning;
        }

        switch (state)
        {
            case UfoState.Approach:  ApproachStep(); break;
            case UfoState.Abducting: AbductionStep(); break;
            case UfoState.Returning: ReturningStep(); break;
        }
    }

    //──────────────────────────────────────────────
    // PHASES
    //──────────────────────────────────────────────

    void ApproachStep()
    {
        if (worldClock.isDay) { state = UfoState.Returning; return; }
        if (!target) { AcquireUniqueTarget(); if (!target) { state = UfoState.Returning; return; } }

        Vector3 next = StepSine();
        MoveAndFace(next);

        Vector3 cowXZ = new Vector3(target.position.x, 0, target.position.z);
        Vector3 meXZ  = new Vector3(transform.position.x, 0, transform.position.z);

        if (Vector3.Distance(cowXZ, meXZ) <= abductRangeXZ)
            BeginAbduction();
    }

    void AbductionStep()
    {
        if (worldClock.isDay) { state = UfoState.Returning; return; }

        if (spotLight)
            spotLight.intensity = Mathf.Lerp(spotLight.intensity, abductionLightIntensity, Time.deltaTime * 2f);

        transform.Rotate(Vector3.up, 180f * Time.deltaTime, Space.World);
        abductTimer += Time.deltaTime;

        if (cowDestroyed && abductTimer >= (cowLiftDuration + hoverBeforeLiftTime + postAbductionDelay))
        {
            StartCoroutine(BlinkAndReturn());
            state = UfoState.Returning;
        }
    }

    void ReturningStep()
    {
        Vector3 dest = new Vector3(spawnPoint.x, flyHeight, spawnPoint.z);
        MoveAndFace(Vector3.MoveTowards(transform.position, dest, moveSpeed * Time.deltaTime));
        if (spotLight) spotLight.intensity = Mathf.MoveTowards(spotLight.intensity, 0f, Time.deltaTime * (abductionLightIntensity + 2f));
        if ((transform.position - dest).sqrMagnitude < 0.1f * 0.1f)
            Destroy(gameObject);
    }

    //──────────────────────────────────────────────
    // ABDUCTION
    //──────────────────────────────────────────────

    void BeginAbduction()
    {
        if (state == UfoState.Abducting) return;

        state = UfoState.Abducting;
        abductTimer = 0f;

        if (spotLight) spotLight.enabled = true;
        if (target && abductionRoutine == null)
            abductionRoutine = StartCoroutine(AbductionSequence(target));
    }

    IEnumerator AbductionSequence(Transform cow)
    {
        yield return new WaitForSeconds(hoverBeforeLiftTime);

        liftingCow = cow;
        Vector3 start = cow.position;
        Vector3 end = new Vector3(cow.position.x, flyHeight, cow.position.z);
        float t = 0f;

        Rigidbody rb = cow.GetComponent<Rigidbody>();
        if (!rb) rb = cow.gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;

        while (t < 1f)
        {
            if (!cow) yield break;
            cow.position = Vector3.Lerp(start, end, t);
            t += Time.deltaTime / cowLiftDuration;
            yield return null;
        }

        if (cow)
        {
            Destroy(cow.gameObject);
            cowDestroyed = true;
            if (AbductionTracker_.Instance)
                AbductionTracker_.Instance.RegisterAbduction();
        }

        liftingCow = null;
        if (target && takenTargets.Contains(target))
            takenTargets.Remove(target);
    }

    IEnumerator BlinkAndReturn()
    {
        if (spotLight)
        {
            Color original = spotLight.color;
            spotLight.color = abductionCompleteColor;
            for (int i = 0; i < blinkCount; i++)
            {
                spotLight.enabled = true;  yield return new WaitForSeconds(blinkSpeed);
                spotLight.enabled = false; yield return new WaitForSeconds(blinkSpeed);
            }
            spotLight.enabled = true;
            spotLight.color = original;
        }
    }

    void OnDestroy()
    {
        // If UFO destroyed mid-abduction → drop cow safely
        if (liftingCow && liftingCow.gameObject)
        {
            Rigidbody rb = liftingCow.GetComponent<Rigidbody>();
            if (!rb) rb = liftingCow.gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = false;
            rb.useGravity = true;

            // use safe runner so we don’t call StartCoroutine on an inactive UFO
            CoroutineRunner.Instance.StartCoroutine(ResetCowAfterFall(rb, liftingCow));
        }

        if (target && takenTargets.Contains(target))
            takenTargets.Remove(target);

        if (spawnerRef) spawnerRef.NotifyUfoDestroyed(this);
    }

    IEnumerator ResetCowAfterFall(Rigidbody rb, Transform cow)
    {
        yield return new WaitForSeconds(fallDelay);
        if (rb)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
        if (cow && takenTargets.Contains(cow))
            takenTargets.Remove(cow);
    }

    //──────────────────────────────────────────────
    // SINE MOVEMENT
    //──────────────────────────────────────────────

    Vector3 StepSine()
    {
        Vector3 pos = transform.position;
        Vector3 centerXZ = new Vector3(target.position.x, 0, target.position.z);
        Vector3 selfXZ = new Vector3(pos.x, 0, pos.z);
        Vector3 forward = (centerXZ - selfXZ).normalized;
        Vector3 right = Vector3.Cross(Vector3.up, forward);

        sineTravel += moveSpeed * Time.deltaTime;
        float lateral = Mathf.Sin(sineTravel * sineFrequency) * sineAmplitude;

        Vector3 desiredXZ = selfXZ + forward * (moveSpeed * Time.deltaTime) + right * lateral * Time.deltaTime;

        return new Vector3(desiredXZ.x, flyHeight, desiredXZ.z);
    }

    void MoveAndFace(Vector3 next)
    {
        Vector3 vel = next - transform.position;
        transform.position = Vector3.MoveTowards(transform.position, next, moveSpeed * Time.deltaTime);
        if (vel.sqrMagnitude > 1e-6f)
        {
            Vector3 fwd = new Vector3(vel.x, 0, vel.z).normalized;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(fwd, Vector3.up), rotationSpeed * Time.deltaTime);
        }
    }

    //──────────────────────────────────────────────
    // UNIQUE TARGET
    //──────────────────────────────────────────────

    void AcquireUniqueTarget()
    {
        GameObject[] cows = GameObject.FindGameObjectsWithTag(targetTag);
        if (cows.Length == 0) { target = null; return; }

        List<Transform> untaken = new List<Transform>();
        foreach (var c in cows)
            if (!takenTargets.Contains(c.transform))
                untaken.Add(c.transform);

        Transform chosen = null;
        if (untaken.Count > 0)
            chosen = untaken[Random.Range(0, untaken.Count)];
        else
            chosen = cows[Random.Range(0, cows.Length)].transform;

        target = chosen;
        if (target && !takenTargets.Contains(target))
            takenTargets.Add(target);
    }
}
