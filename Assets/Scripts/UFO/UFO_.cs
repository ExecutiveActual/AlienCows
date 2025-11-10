using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class UFO_ : MonoBehaviour
{
    public enum UFOType { A_CircleSpiral, B_Parabola, C_Sine, D_Hyperbola, E_InverseX }
    private enum UfoState { IdleDay, Approach, Abducting, Returning }

    [Header("Core")]
    [Tooltip("Select the behavior type for this UFO instance.")]
    public UFOType type = UFOType.C_Sine;

    [Tooltip("World time reference. If empty, this script will try to find one.")]
    public WorldClock_ worldClock;

    [Tooltip("Spawner that created this UFO (set automatically by spawner).")]
    public UFOSpawner_ spawnerRef;

    [Tooltip("Tag used to find targets (default: Cow).")]
    public string targetTag = "Cow";

    [Tooltip("Always draw the predicted path in Scene View.")]
    public bool showPathGizmo = true;

    [Tooltip("Allow only one UFO to chase a specific Cow at a time.")]
    public bool useUniqueTargets = true;

    [Header("Movement (Shared)")]
    [Tooltip("Forward speed while approaching the target.")]
    public float moveSpeed = 10f;

    [Tooltip("Smooth turning toward movement direction.")]
    public float rotationSpeed = 8f;

    [Tooltip("Nominal flight height (Y) while moving.")]
    public float flyHeight = 12f;

    [Tooltip("Distance at which the UFO is considered directly above the cow to start abduction.")]
    public float abductRangeXZ = 2.5f;

    [Tooltip("Vertical offset above the cow to hold during abduction.")]
    public float abductHoverHeight = 10f;

    [Header("Type A (Circle → Spiral)")]
    public float orbitRadius = 10f;
    public float orbitDegPerSec = 120f;
    public float spiralInwardSpeed = 3f;

    [Header("Type B (Parabola)")]
    [Tooltip("Half-width of the parabola toward the vertex at the cow.")]
    public float parabolaWidth = 8f;
    [Tooltip("Arc height of the parabola.")]
    public float parabolaArcHeight = 6f;

    [Header("Type C (Sine Zig-Zag toward Cow)")]
    [Tooltip("Lateral amplitude of the sine zig-zag (XZ plane).")]
    public float sineAmplitude = 6f;
    [Tooltip("Spatial frequency of the sine wave along the forward axis.")]
    public float sineFrequency = 0.5f;
    [Tooltip("Vertical bob amplitude during sine approach.")]
    public float sineBobAmplitudeY = 1.5f;
    [Tooltip("Vertical bob frequency during sine approach.")]
    public float sineBobFrequency = 1.25f;

    [Header("Type D (Hyperbola)")]
    public float hyperbolaScale = 15f;

    [Header("Type E (1/x)")]
    public float inverseScale = 20f;

    [Header("Spotlight / Abduction")]
    [Tooltip("Spotlight (child Light). Leave null to auto-find first child Light.")]
    public Light spotLight;
    [Tooltip("Target intensity reached during the 10s abduction spin.")]
    public float abductionLightIntensity = 8f;
    [Tooltip("Spotlight color after abduction completes (before blinking).")]
    public Color abductionCompleteColor = Color.cyan;
    [Tooltip("How many blinks after abduction completes.")]
    public int blinkCount = 3;
    [Tooltip("Seconds for each on/off blink step.")]
    public float blinkSpeed = 0.25f;

    [Header("Abduction Timing")]
    [Tooltip("Seconds the UFO spins over the cow (also equals number of rotations).")]
    public float abductionRotateDuration = 10f; // 10 sec
    [Tooltip("Total full rotations during abduction.")]
    public int rotationsDuringAbduction = 10;    // 10 rotations → 360 deg/s

    // Runtime
    private static readonly List<Transform> takenTargets = new List<Transform>();

    private Transform target;            // assigned cow
    private Vector3 spawnPoint;          // where this UFO spawned
    private Vector3 lastVelocity;
    private Vector3 planarForward;       // fallback forward on XZ
    private UfoState state = UfoState.Approach;

    // Type A runtime
    private float orbitAngle;
    private float orbitAngleAccum;
    private float currentOrbitRadius;
    private bool spiraling;

    // Sine phase tracking
    private float sineTravel;            // along-forward distance for phase
    private float sineTime;              // time accumulator for vertical bob

    // Abduction runtime
    private float abductTimer;
    private Quaternion startAbductRotation;
    private bool cowDestroyed;

    private void Awake()
    {
        if (worldClock == null) worldClock = FindObjectOfType<WorldClock_>();
        if (spotLight == null)  spotLight = GetComponentInChildren<Light>(true);
    }

    private void OnEnable()
    {
        spawnPoint = transform.position;  // remember where we came from
        AcquireUniqueTarget();
        ResetTypeRuntime();
        cowDestroyed = false;

        // place at nominal height in edit/play
        var p = transform.position;
        transform.position = new Vector3(p.x, flyHeight, p.z);

        // prepare spotlight off
        if (spotLight != null) spotLight.intensity = 0f;
    }

    private void Update()
    {
        // Edit-time: keep at height and draw gizmos only
        if (!Application.isPlaying)
        {
            var p = transform.position;
            transform.position = new Vector3(p.x, flyHeight, p.z);
            return;
        }

        if (worldClock == null)
        {
            Debug.Log("WorldClock_ not found for UFO_.");
            return;
        }

        // Day behavior: abort/idle and despawn policy
        if (worldClock.isDay)
        {
            if (state != UfoState.IdleDay)
            {
                // Abort any abduction and go home instantly during day
                StopAllCoroutines();
                state = UfoState.Returning;
            }
        }

        // If target lost, try reacquire (unique)
        if ((target == null || !target.gameObject) && state == UfoState.Approach && !worldClock.isDay)
        {
            AcquireUniqueTarget();
            if (target == null) state = UfoState.Returning; // nothing to chase
        }

        switch (state)
        {
            case UfoState.IdleDay:
                IdleDayStep();
                break;

            case UfoState.Approach:
                ApproachStep();
                break;

            case UfoState.Abducting:
                AbductionStep();
                break;

            case UfoState.Returning:
                ReturningStep();
                break;
        }
    }

    // ---------------- States ----------------

    private void IdleDayStep()
    {
        // Hover at height, gentle rotation could be added if desired
        var p = transform.position;
        transform.position = new Vector3(p.x, flyHeight, p.z);
    }

    private void ApproachStep()
    {
        if (worldClock.isDay) { state = UfoState.Returning; return; }
        if (target == null) { state = UfoState.Returning; return; }

        Vector3 nextPos;

        switch (type)
        {
            case UFOType.A_CircleSpiral: nextPos = StepTypeA_CircleSpiralApproach(); break;
            case UFOType.B_Parabola:     nextPos = StepTypeB_ParabolaApproach();     break;
            case UFOType.C_Sine:         nextPos = StepTypeC_SineApproach();         break;
            case UFOType.D_Hyperbola:    nextPos = StepTypeD_HyperbolaApproach();    break;
            case UFOType.E_InverseX:     nextPos = StepTypeE_InverseXApproach();     break;
            default:                     nextPos = transform.position;               break;
        }

        // Keep at fly height unless sine bobbing
        if (type != UFOType.C_Sine) nextPos.y = flyHeight;

        MoveAndFace(nextPos);

        // Check abduction trigger (XZ proximity and hover height)
        Vector3 cowXZ = new Vector3(target.position.x, 0f, target.position.z);
        Vector3 meXZ  = new Vector3(transform.position.x, 0f, transform.position.z);
        float distXZ  = Vector3.Distance(cowXZ, meXZ);

        if (distXZ <= abductRangeXZ && !worldClock.isDay)
        {
            BeginAbduction();
        }
    }

    private void AbductionStep()
    {
        if (worldClock.isDay)
        {
            AbortAbductionAndReturn();
            return;
        }
        if (target == null && !cowDestroyed)
        {
            // Cow vanished; abort gracefully
            AbortAbductionAndReturn();
            return;
        }

        // Hold position directly above cow and spin
        Vector3 hoverPos = (target != null)
            ? new Vector3(target.position.x, target.position.y + abductHoverHeight, target.position.z)
            : new Vector3(transform.position.x, flyHeight, transform.position.z);

        // Slight, smooth descend toward hover position to sell the beam
        transform.position = Vector3.Lerp(transform.position, hoverPos, Time.deltaTime * 2f);

        // Spin: 10 rotations over duration
        float degPerSec = (rotationsDuringAbduction / Mathf.Max(0.01f, abductionRotateDuration)) * 360f;
        transform.Rotate(Vector3.up, degPerSec * Time.deltaTime, Space.World);

        // Ramp spotlight intensity
        if (spotLight != null)
        {
            spotLight.intensity = Mathf.Lerp(spotLight.intensity, abductionLightIntensity, Time.deltaTime * 2f);
        }

        abductTimer += Time.deltaTime;
        if (abductTimer >= abductionRotateDuration)
        {
            // Destroy the cow (once)
            if (target != null && !cowDestroyed)
            {
                cowDestroyed = true;
                // release from unique list
                if (useUniqueTargets && takenTargets.Contains(target))
                    takenTargets.Remove(target);

                Destroy(target.gameObject);
                target = null;
            }

            // Blink the light, then return
            StartCoroutine(BlinkAndReturn());
            state = UfoState.Returning;
        }
    }

    private void ReturningStep()
    {
        // Fly back to spawn point, fade light out, despawn
        Vector3 nextPos = Vector3.MoveTowards(transform.position, new Vector3(spawnPoint.x, flyHeight, spawnPoint.z), moveSpeed * Time.deltaTime);
        MoveAndFace(nextPos);

        // Fade light down smoothly
        if (spotLight != null)
        {
            spotLight.intensity = Mathf.MoveTowards(spotLight.intensity, 0f, Time.deltaTime * (abductionLightIntensity + 2f));
        }

        if (Vector3.SqrMagnitude(transform.position - new Vector3(spawnPoint.x, flyHeight, spawnPoint.z)) < 0.05f)
        {
            // reached spawn; destroy self
            Destroy(gameObject);
        }
    }

    // ---------------- Abduction helpers ----------------

    private void BeginAbduction()
    {
        state = UfoState.Abducting;
        abductTimer = 0f;
        startAbductRotation = transform.rotation;

        // Turn on spotlight if available
        if (spotLight != null)
        {
            spotLight.enabled = true;
            // keep current color; intensity ramps in AbductionStep
        }

        // Snap directly above cow baseline as target hover
        if (target != null)
        {
            Vector3 hover = new Vector3(target.position.x, target.position.y + abductHoverHeight, target.position.z);
            transform.position = new Vector3(transform.position.x, hover.y, transform.position.z);
        }
    }

    private void AbortAbductionAndReturn()
    {
        StopAllCoroutines();
        state = UfoState.Returning;
    }

    private IEnumerator BlinkAndReturn()
    {
        if (spotLight != null)
        {
            // Change to completion color, then blink 3 times
            Color originalColor = spotLight.color;
            spotLight.color = abductionCompleteColor;

            for (int i = 0; i < blinkCount; i++)
            {
                spotLight.enabled = true;
                yield return new WaitForSeconds(blinkSpeed);
                spotLight.enabled = false;
                yield return new WaitForSeconds(blinkSpeed);
            }

            // Restore enabled; intensity fade is handled in ReturningStep
            spotLight.enabled = true;
            spotLight.color = originalColor;
        }
        yield break;
    }

    // ---------------- Movement primitives ----------------

    private void MoveAndFace(Vector3 nextPos)
    {
        Vector3 delta = nextPos - transform.position;
        Vector3 vel = (delta.sqrMagnitude > 1e-8f)
            ? delta / Mathf.Max(Time.deltaTime, 1e-4f)
            : lastVelocity;

        transform.position = Vector3.MoveTowards(transform.position, nextPos, moveSpeed * Time.deltaTime);

        if (vel.sqrMagnitude > 1e-6f)
        {
            lastVelocity = vel;
            Vector3 fwd = new Vector3(vel.x, 0f, vel.z).normalized;
            Quaternion targetRot = Quaternion.LookRotation(fwd, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }
    }

    // A: orbit until within radius, then spiral inward toward the cow center (Approach phase)
    private Vector3 StepTypeA_CircleSpiralApproach()
    {
        Vector3 pos = transform.position;
        Vector3 center = new Vector3(target.position.x, flyHeight, target.position.z);
        Vector3 toCenter = center - pos; toCenter.y = 0f;
        float dist = toCenter.magnitude;

        if (!spiraling)
        {
            // close in until within orbit radius
            if (dist > orbitRadius * 1.05f)
            {
                return pos + toCenter.normalized * moveSpeed * Time.deltaTime;
            }

            // orbit
            orbitAngle += orbitDegPerSec * Mathf.Deg2Rad * Time.deltaTime;
            orbitAngleAccum += orbitDegPerSec * Time.deltaTime;

            Vector3 radial = dist > 1e-3f ? toCenter.normalized : Vector3.forward;
            Vector3 tangent = Vector3.Cross(Vector3.up, radial).normalized;
            Vector3 desired = center + (radial * orbitRadius * Mathf.Cos(orbitAngle))
                                      + (tangent * orbitRadius * Mathf.Sin(orbitAngle));

            if (orbitAngleAccum >= 180f) spiraling = true;
            return desired;
        }
        else
        {
            // spiral inward
            currentOrbitRadius = Mathf.Max(0f, currentOrbitRadius - spiralInwardSpeed * Time.deltaTime);
            orbitAngle += orbitDegPerSec * Mathf.Deg2Rad * Time.deltaTime;

            Vector3 radial = dist > 1e-3f ? toCenter.normalized : Vector3.forward;
            Vector3 tangent = Vector3.Cross(Vector3.up, radial).normalized;

            Vector3 desired = center + (radial * currentOrbitRadius * Mathf.Cos(orbitAngle))
                                      + (tangent * currentOrbitRadius * Mathf.Sin(orbitAngle));

            return desired;
        }
    }

    // B: parabolic arc toward vertex at cow
    private Vector3 StepTypeB_ParabolaApproach()
    {
        Vector3 pos = transform.position;
        Vector3 center = new Vector3(target.position.x, flyHeight, target.position.z);

        Vector3 forwardToTarget = center - pos; forwardToTarget.y = 0f;
        if (forwardToTarget.sqrMagnitude < 1e-6f) forwardToTarget = planarForward;
        Vector3 xAxis = forwardToTarget.normalized;
        Vector3 zAxis = Vector3.Cross(Vector3.up, xAxis);

        // Advance along x toward vertex (x → 0), z = k x^2
        Vector3 local = WorldToLocal(pos, center, xAxis, zAxis);
        float step = moveSpeed * Time.deltaTime;
        float dir = Mathf.Sign(-local.x);
        local.x += dir * step;

        float k = Mathf.Approximately(parabolaWidth, 0f) ? 0.001f : (parabolaArcHeight / (parabolaWidth * parabolaWidth));
        local.z = k * (local.x * local.x);

        return LocalToWorld(local, center, xAxis, zAxis);
    }

    // C: true sine zig-zag in XZ toward cow, with gentle Y bob
    private Vector3 StepTypeC_SineApproach()
    {
        Vector3 pos = transform.position;
        Vector3 centerXZ = new Vector3(target.position.x, 0f, target.position.z);
        Vector3 selfXZ   = new Vector3(pos.x, 0f, pos.z);

        // Forward axis toward cow on XZ
        Vector3 forward = (centerXZ - selfXZ).normalized;
        if (forward.sqrMagnitude < 1e-6f) forward = Vector3.forward;
        Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;

        // Advance along the forward axis
        sineTravel += moveSpeed * Time.deltaTime;

        // Lateral sine offset
        float lateral = Mathf.Sin(sineTravel * sineFrequency) * sineAmplitude;

        // Base along-forward move
        Vector3 desiredXZ = selfXZ + forward * (moveSpeed * Time.deltaTime);

        // Apply zig-zag lateral
        desiredXZ += right * lateral * Time.deltaTime; // gradual lateral so it’s not teleporty

        // Gentle vertical bob
        sineTime += Time.deltaTime;
        float y = flyHeight + Mathf.Sin(sineTime * Mathf.PI * 2f * sineBobFrequency) * sineBobAmplitudeY;

        return new Vector3(desiredXZ.x, y, desiredXZ.z);
    }

    // D: hyperbolic approach (accelerate then brake near target)
    private Vector3 StepTypeD_HyperbolaApproach()
    {
        Vector3 pos = transform.position;
        Vector3 center = new Vector3(target.position.x, flyHeight, target.position.z);

        Vector3 forwardToTarget = center - pos; forwardToTarget.y = 0f;
        if (forwardToTarget.sqrMagnitude < 1e-6f) forwardToTarget = planarForward;
        Vector3 xAxis = forwardToTarget.normalized;
        Vector3 zAxis = Vector3.Cross(Vector3.up, xAxis);

        Vector3 local = WorldToLocal(pos, center, xAxis, zAxis);
        float step = moveSpeed * Time.deltaTime;
        float dir = Mathf.Sign(-local.x);
        local.x += dir * step;

        float a = Mathf.Max(0.1f, hyperbolaScale);
        float eps = 0.5f;
        local.z = (a * a) / Mathf.Max(eps, Mathf.Abs(local.x));
        return LocalToWorld(local, center, xAxis, zAxis);
    }

    // E: 1/x curved approach
    private Vector3 StepTypeE_InverseXApproach()
    {
        Vector3 pos = transform.position;
        Vector3 center = new Vector3(target.position.x, flyHeight, target.position.z);

        Vector3 forwardToTarget = center - pos; forwardToTarget.y = 0f;
        if (forwardToTarget.sqrMagnitude < 1e-6f) forwardToTarget = planarForward;
        Vector3 xAxis = forwardToTarget.normalized;
        Vector3 zAxis = Vector3.Cross(Vector3.up, xAxis);

        Vector3 local = WorldToLocal(pos, center, xAxis, zAxis);
        float step = moveSpeed * Time.deltaTime;
        float dir = Mathf.Sign(-local.x);
        local.x += dir * step;

        float s = Mathf.Max(0.1f, inverseScale);
        float eps = 0.5f;
        local.z = s / Mathf.Max(eps, Mathf.Abs(local.x));
        return LocalToWorld(local, center, xAxis, zAxis);
    }

    // ---------------- Gizmos ----------------

    private void OnDrawGizmos()
    {
        if (!showPathGizmo) return;

        Transform t = target;
        if (t == null)
        {
            GameObject[] cows = GameObject.FindGameObjectsWithTag(targetTag);
            if (cows != null && cows.Length > 0) t = cows[0].transform;
        }

        Color c = GizmoColorForType(type);
        Gizmos.color = c;

        Vector3 center = t ? new Vector3(t.position.x, flyHeight, t.position.z)
                           : new Vector3(transform.position.x, flyHeight, transform.position.z);

        const int steps = 128;
        Vector3 prev = transform.position; prev.y = flyHeight;

        for (int i = 1; i <= steps; i++)
        {
            float u = i / (float)steps;
            Vector3 p = SamplePath(center, u);
            p.y = (type == UFOType.C_Sine) ? flyHeight + Mathf.Sin(u * Mathf.PI * 4f) * sineBobAmplitudeY : flyHeight;
            Gizmos.DrawLine(prev, p);
            prev = p;
        }
    }

    private Vector3 SamplePath(Vector3 center, float u01)
    {
        Vector3 pos = transform.position; pos.y = flyHeight;
        Vector3 fwd = center - pos; fwd.y = 0f;
        if (fwd.sqrMagnitude < 1e-6f) fwd = (planarForward.sqrMagnitude < 1e-6f ? Vector3.forward : planarForward);
        fwd.Normalize();
        Vector3 xAxis = fwd;
        Vector3 zAxis = Vector3.Cross(Vector3.up, xAxis);

        switch (type)
        {
            case UFOType.A_CircleSpiral:
            {
                float ang = Mathf.Lerp(0, Mathf.PI * 2f, u01);
                float r = Mathf.Lerp(orbitRadius, 0f, Mathf.SmoothStep(0f, 1f, Mathf.Max(0f, u01 - 0.5f) * 2f));
                Vector3 radial = (pos - center); radial.y = 0f; if (radial.sqrMagnitude < 1e-6f) radial = xAxis; radial.Normalize();
                Vector3 tangent = Vector3.Cross(Vector3.up, radial).normalized;
                return center + radial * (r * Mathf.Cos(ang)) + tangent * (r * Mathf.Sin(ang));
            }

            case UFOType.B_Parabola:
            {
                float x = Mathf.Lerp(-parabolaWidth, parabolaWidth, u01);
                float k = Mathf.Approximately(parabolaWidth, 0f) ? 0.001f : (parabolaArcHeight / (parabolaWidth * parabolaWidth));
                float z = k * x * x;
                return LocalToWorld(new Vector3(x, 0f, z), center, xAxis, zAxis);
            }

            case UFOType.C_Sine:
            {
                float L = 40f;
                Vector3 baseP = pos + xAxis * (u01 * L - L * 0.5f);
                float lateral = Mathf.Sin((u01 * L) * sineFrequency) * sineAmplitude;
                baseP += zAxis * lateral * 0.35f; // lighter in gizmo
                return baseP;
            }

            case UFOType.D_Hyperbola:
            {
                float x = Mathf.Lerp(-20f, 20f, u01);
                float a2 = Mathf.Max(0.1f, hyperbolaScale);
                float z = (a2 * a2) / Mathf.Max(0.5f, Mathf.Abs(x));
                return LocalToWorld(new Vector3(x, 0f, z), center, xAxis, zAxis);
            }

            case UFOType.E_InverseX:
            {
                float x = Mathf.Lerp(-20f, 20f, u01);
                float z = Mathf.Max(0.1f, inverseScale) / Mathf.Max(0.5f, Mathf.Abs(x));
                return LocalToWorld(new Vector3(x, 0f, z), center, xAxis, zAxis);
            }
        }
        return pos;
    }

    private static Color GizmoColorForType(UFOType t)
    {
        switch (t)
        {
            case UFOType.A_CircleSpiral: return new Color(0.2f, 0.8f, 1f, 1f);
            case UFOType.B_Parabola:     return new Color(1f, 0.85f, 0.2f, 1f);
            case UFOType.C_Sine:         return new Color(0.6f, 1f, 0.4f, 1f);
            case UFOType.D_Hyperbola:    return new Color(1f, 0.4f, 0.6f, 1f);
            case UFOType.E_InverseX:     return new Color(0.9f, 0.55f, 1f, 1f);
        }
        return Color.white;
    }

    // ---------------- Targeting ----------------

    private void AcquireTarget()
    {
        GameObject[] candidates = GameObject.FindGameObjectsWithTag(targetTag);
        if (candidates == null || candidates.Length == 0) { target = null; return; }

        float best = float.PositiveInfinity;
        Transform bestT = null;
        Vector3 self = transform.position;
        foreach (var c in candidates)
        {
            float d = (c.transform.position - self).sqrMagnitude;
            if (d < best) { best = d; bestT = c.transform; }
        }
        target = bestT;
    }

    private void AcquireUniqueTarget()
    {
        if (!useUniqueTargets) { AcquireTarget(); return; }

        GameObject[] cows = GameObject.FindGameObjectsWithTag(targetTag);
        if (cows == null || cows.Length == 0) { target = null; return; }

        List<Transform> untaken = new List<Transform>();
        foreach (var c in cows)
            if (!takenTargets.Contains(c.transform)) untaken.Add(c.transform);

        Transform chosen = null;
        float bestDist = float.PositiveInfinity;
        Vector3 selfPos = transform.position;

        if (untaken.Count > 0)
        {
            foreach (var c in untaken)
            {
                float d = (c.position - selfPos).sqrMagnitude;
                if (d < bestDist) { bestDist = d; chosen = c.transform; }
            }
        }
        else
        {
            foreach (var c in cows)
            {
                float d = (c.transform.position - selfPos).sqrMagnitude;
                if (d < bestDist) { bestDist = d; chosen = c.transform; }
            }
        }

        if (chosen != null)
        {
            target = chosen;
            if (!takenTargets.Contains(chosen)) takenTargets.Add(chosen);
        }
    }

    private void ResetTypeRuntime()
    {
        orbitAngle = 0f;
        orbitAngleAccum = 0f;
        currentOrbitRadius = orbitRadius;
        spiraling = false;

        planarForward = transform.forward; planarForward.y = 0f;
        if (planarForward.sqrMagnitude < 1e-6f) planarForward = Vector3.forward;
        planarForward.Normalize();

        sineTravel = 0f;
        sineTime = 0f;
        abductTimer = 0f;
    }

    private static Vector3 WorldToLocal(Vector3 world, Vector3 center, Vector3 xAxis, Vector3 zAxis)
    {
        Vector3 d = world - center;
        float lx = Vector3.Dot(d, xAxis);
        float lz = Vector3.Dot(d, zAxis);
        return new Vector3(lx, 0f, lz);
    }

    private static Vector3 LocalToWorld(Vector3 local, Vector3 center, Vector3 xAxis, Vector3 zAxis)
    {
        return center + xAxis * local.x + zAxis * local.z;
    }

    private void OnDisable()
    {
        if (useUniqueTargets && target != null && takenTargets.Contains(target))
            takenTargets.Remove(target);
    }

    private void OnDestroy()
    {
        if (useUniqueTargets && target != null && takenTargets.Contains(target))
            takenTargets.Remove(target);

        if (spawnerRef != null)
            spawnerRef.NotifyUfoDestroyed(this);
    }
}
