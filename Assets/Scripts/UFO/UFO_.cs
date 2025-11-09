using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class UFO_ : MonoBehaviour
{
    public enum UFOType { A_CircleSpiral, B_Parabola, C_Sine, D_Hyperbola, E_InverseX }

    [Header("Core")]
    [Tooltip("Select the behavior type for this UFO instance.")]
    public UFOType type = UFOType.A_CircleSpiral;

    [Tooltip("World time reference. If left empty, the script will try to find one in the scene.")]
    public WorldClock_ worldClock;

    [Tooltip("Spawner that created this UFO (set automatically).")]
    public UFOSpawner_ spawnerRef;

    [Tooltip("Tag used to find targets. Default is 'Cow'.")]
    public string targetTag = "Cow";

    [Tooltip("Always draw the predicted path in Scene view.")]
    public bool showPathGizmo = true;

    [Tooltip("Allow only one UFO to chase a specific Cow at a time.")]
    public bool useUniqueTargets = true;

    [Header("Common Movement")]
    [Tooltip("How quickly the UFO translates along its path.")]
    public float moveSpeed = 6f;

    [Tooltip("Y height to maintain while flying.")]
    public float flyHeight = 12f;

    [Tooltip("How quickly the UFO rotates to face its movement direction.")]
    public float rotationSpeed = 8f;

    [Header("Type A (Circle → Spiral)")]
    [Tooltip("Distance from target to begin orbiting.")]
    public float orbitRadius = 10f;

    [Tooltip("Angular speed while orbiting (degrees per second).")]
    public float orbitDegPerSec = 90f;

    [Tooltip("Spiral speed (units per second) after half a lap.")]
    public float spiralInwardSpeed = 2f;

    [Header("Type B (Parabola)")]
    [Tooltip("Controls parabola width (larger is wider/flatter).")]
    public float parabolaWidth = 8f;

    [Tooltip("Max arc height (units) away from the straight line to the target.")]
    public float parabolaArcHeight = 6f;

    [Header("Type C (Sine)")]
    [Tooltip("Amplitude of the sine deviation (units).")]
    public float sineAmplitude = 6f;

    [Tooltip("Spatial frequency of the sine wave.")]
    public float sineFrequency = 0.5f;

    [Header("Type D (Hyperbola)")]
    [Tooltip("Scale factor for hyperbola shape.")]
    public float hyperbolaScale = 15f;

    [Header("Type E (1/x)")]
    [Tooltip("Scale factor for 1/x shape.")]
    public float inverseScale = 20f;

    // Runtime
    private Transform target;
    private Vector3 lastVelocity;
    private float orbitAngle;
    private float orbitAngleAccum;
    private float currentOrbitRadius;
    private bool spiraling;
    private Vector3 planarForward;

    // --- Unique target system ---
    private static readonly List<Transform> takenTargets = new List<Transform>();

    private void Awake()
    {
        if (worldClock == null) worldClock = FindObjectOfType<WorldClock_>();
    }

    private void OnEnable()
    {
        AcquireUniqueTarget();
        ResetTypeRuntime();
    }

    private void Update()
    {
        if (!Application.isPlaying)
        {
            var p = transform.position;
            transform.position = new Vector3(p.x, flyHeight, p.z);
            return;
        }

        if (worldClock == null) { Debug.Log("WorldClock_ not found for UFO_."); return; }

        if (worldClock.isDay)
        {
            var p = transform.position;
            transform.position = new Vector3(p.x, flyHeight, p.z);
            return;
        }

        if (target == null) AcquireUniqueTarget();

        Vector3 nextPos = transform.position;

        switch (type)
        {
            case UFOType.A_CircleSpiral: nextPos = StepTypeA_CircleSpiral(); break;
            case UFOType.B_Parabola:     nextPos = StepTypeB_Parabola(); break;
            case UFOType.C_Sine:         nextPos = StepTypeC_Sine(); break;
            case UFOType.D_Hyperbola:    nextPos = StepTypeD_Hyperbola(); break;
            case UFOType.E_InverseX:     nextPos = StepTypeE_InverseX(); break;
        }

        nextPos.y = flyHeight;

        Vector3 vel = (nextPos - transform.position) / Mathf.Max(Time.deltaTime, 1e-4f);
        if (vel.sqrMagnitude > 1e-6f) lastVelocity = vel;

        transform.position = Vector3.MoveTowards(transform.position, nextPos, moveSpeed * Time.deltaTime);

        if (lastVelocity.sqrMagnitude > 1e-6f)
        {
            Vector3 fwd = new Vector3(lastVelocity.x, 0f, lastVelocity.z).normalized;
            if (fwd.sqrMagnitude > 1e-6f)
            {
                Quaternion targetRot = Quaternion.LookRotation(fwd, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
            }
        }
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
        if (!useUniqueTargets)
        {
            AcquireTarget();
            return;
        }

        GameObject[] cows = GameObject.FindGameObjectsWithTag(targetTag);
        if (cows == null || cows.Length == 0)
        {
            target = null;
            return;
        }

        List<Transform> untaken = new List<Transform>();
        foreach (var c in cows)
        {
            if (!takenTargets.Contains(c.transform))
                untaken.Add(c.transform);
        }

        Transform chosen = null;
        float bestDist = float.PositiveInfinity;
        Vector3 selfPos = transform.position;

        if (untaken.Count > 0)
        {
            foreach (var c in untaken)
            {
                float d = (c.position - selfPos).sqrMagnitude;
                if (d < bestDist)
                {
                    bestDist = d;
                    chosen = c.transform;
                }
            }
        }
        else
        {
            foreach (var c in cows)
            {
                float d = (c.transform.position - selfPos).sqrMagnitude;
                if (d < bestDist)
                {
                    bestDist = d;
                    chosen = c.transform;
                }
            }
        }

        if (chosen != null)
        {
            target = chosen;
            if (!takenTargets.Contains(chosen))
                takenTargets.Add(chosen);
        }
    }

    private void ResetTypeRuntime()
    {
        orbitAngle = 0f;
        orbitAngleAccum = 0f;
        currentOrbitRadius = orbitRadius;
        spiraling = false;
        planarForward = transform.forward;
        planarForward.y = 0f;
        if (planarForward.sqrMagnitude < 1e-6f) planarForward = Vector3.forward;
        planarForward.Normalize();
    }

    // ---------------- Type A: Circle + Spiral ----------------
    private Vector3 StepTypeA_CircleSpiral()
    {
        Vector3 pos = transform.position;
        Vector3 center = target ? new Vector3(target.position.x, flyHeight, target.position.z) : pos;
        Vector3 toCenter = center - pos; toCenter.y = 0f;
        float dist = toCenter.magnitude;

        if (!spiraling)
        {
            if (dist > orbitRadius * 1.05f)
                return pos + toCenter.normalized * moveSpeed * Time.deltaTime;

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
            currentOrbitRadius = Mathf.Max(0f, currentOrbitRadius - spiralInwardSpeed * Time.deltaTime);
            orbitAngle += orbitDegPerSec * Mathf.Deg2Rad * Time.deltaTime;

            Vector3 radial = dist > 1e-3f ? toCenter.normalized : Vector3.forward;
            Vector3 tangent = Vector3.Cross(Vector3.up, radial).normalized;

            Vector3 desired = center + (radial * currentOrbitRadius * Mathf.Cos(orbitAngle))
                                        + (tangent * currentOrbitRadius * Mathf.Sin(orbitAngle));

            if (currentOrbitRadius <= 0.25f || (desired - center).sqrMagnitude < 0.1f * 0.1f)
                desired = center;

            return desired;
        }
    }

    // ---------------- Type B: Parabola ----------------
    private Vector3 StepTypeB_Parabola()
    {
        Vector3 pos = transform.position;
        Vector3 center = target ? new Vector3(target.position.x, flyHeight, target.position.z) : pos;
        Vector3 forwardToTarget = center - pos; forwardToTarget.y = 0f;
        if (forwardToTarget.sqrMagnitude < 1e-6f) forwardToTarget = planarForward;
        Vector3 xAxis = forwardToTarget.normalized;
        Vector3 zAxis = Vector3.Cross(Vector3.up, xAxis);
        Vector3 local = WorldToLocal(pos, center, xAxis, zAxis);
        float step = moveSpeed * Time.deltaTime;
        float dir = Mathf.Sign(-local.x);
        local.x += dir * step;
        float k = Mathf.Approximately(parabolaWidth, 0f) ? 0.001f : (parabolaArcHeight / (parabolaWidth * parabolaWidth));
        local.z = k * (local.x * local.x);
        return LocalToWorld(local, center, xAxis, zAxis);
    }

    // ---------------- Type C: Sine ----------------
    private Vector3 StepTypeC_Sine()
    {
        Vector3 pos = transform.position;
        Vector3 center = target ? new Vector3(target.position.x, flyHeight, target.position.z) : pos;
        Vector3 forwardToTarget = center - pos; forwardToTarget.y = 0f;
        if (forwardToTarget.sqrMagnitude < 1e-6f) forwardToTarget = planarForward;
        Vector3 xAxis = forwardToTarget.normalized;
        Vector3 zAxis = Vector3.Cross(Vector3.up, xAxis);
        Vector3 local = WorldToLocal(pos, center, xAxis, zAxis);
        float step = moveSpeed * Time.deltaTime;
        float dir = Mathf.Sign(-local.x);
        local.x += dir * step;
        float phase = local.x * sineFrequency;
        local.z = Mathf.Sin(phase) * sineAmplitude;
        return LocalToWorld(local, center, xAxis, zAxis);
    }

    // ---------------- Type D: Hyperbola ----------------
    private Vector3 StepTypeD_Hyperbola()
    {
        Vector3 pos = transform.position;
        Vector3 center = target ? new Vector3(target.position.x, flyHeight, target.position.z) : pos;
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
        local.z *= Mathf.Sign(local.z == 0 ? 1f : local.z);
        return LocalToWorld(local, center, xAxis, zAxis);
    }

    // ---------------- Type E: 1/x ----------------
    private Vector3 StepTypeE_InverseX()
    {
        Vector3 pos = transform.position;
        Vector3 center = target ? new Vector3(target.position.x, flyHeight, target.position.z) : pos;
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
        local.z *= Mathf.Sign(local.z == 0 ? 1f : local.z);
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
        Vector3 prev = transform.position;
        prev.y = flyHeight;

        for (int i = 1; i <= steps; i++)
        {
            float u = i / (float)steps;
            Vector3 p = SamplePath(center, u);
            p.y = flyHeight;
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
        float pathLen = 40f;
        Vector3 start = pos - xAxis * pathLen * 0.5f;
        Vector3 end = pos + xAxis * pathLen * 0.5f;
        Vector3 pt = Vector3.Lerp(start, end, u01);
        Vector3 local = WorldToLocal(pt, center, xAxis, zAxis);

        switch (type)
        {
            case UFOType.A_CircleSpiral:
                float ang = Mathf.Lerp(0, Mathf.PI * 2f, u01);
                Vector3 radial = (pos - center); radial.y = 0f;
                if (radial.sqrMagnitude < 1e-6f) radial = xAxis;
                radial.Normalize();
                Vector3 tangent = Vector3.Cross(Vector3.up, radial).normalized;
                float r = Mathf.Lerp(orbitRadius, 0f, Mathf.SmoothStep(0f, 1f, Mathf.Max(0f, u01 - 0.5f) * 2f));
                return center + radial * (r * Mathf.Cos(ang)) + tangent * (r * Mathf.Sin(ang));

            case UFOType.B_Parabola:
                float x = Mathf.Lerp(-parabolaWidth, parabolaWidth, u01);
                float k = Mathf.Approximately(parabolaWidth, 0f) ? 0.001f : (parabolaArcHeight / (parabolaWidth * parabolaWidth));
                float z = k * x * x;
                return LocalToWorld(new Vector3(x, 0f, z), center, xAxis, zAxis);

            case UFOType.C_Sine:
                x = Mathf.Lerp(-20f, 20f, u01);
                float phase = x * sineFrequency;
                z = Mathf.Sin(phase) * sineAmplitude;
                return LocalToWorld(new Vector3(x, 0f, z), center, xAxis, zAxis);

            case UFOType.D_Hyperbola:
                x = Mathf.Lerp(-20f, 20f, u01);
                float a2 = Mathf.Max(0.1f, hyperbolaScale);
                z = (a2 * a2) / Mathf.Max(0.5f, Mathf.Abs(x));
                return LocalToWorld(new Vector3(x, 0f, z), center, xAxis, zAxis);

            case UFOType.E_InverseX:
                x = Mathf.Lerp(-20f, 20f, u01);
                z = Mathf.Max(0.1f, inverseScale) / Mathf.Max(0.5f, Mathf.Abs(x));
                return LocalToWorld(new Vector3(x, 0f, z), center, xAxis, zAxis);
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
