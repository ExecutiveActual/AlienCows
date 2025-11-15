using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class UFOControllerEvent : UnityEvent<UFO_Controller> { }

[RequireComponent(typeof(UFO_Abduction))]
[RequireComponent(typeof(UFO_Health))]
[RequireComponent(typeof(AudioSource))]
public class UFO_Controller : MonoBehaviour
{
    [Header("Movement")]
    public float forwardSpeed = 6f;
    public float rotationSpeed = 6f;
    public float lateralAmplitude = 2f;
    public float lateralFrequency = 1.5f;
    public float fixedY = 8f;
    public float abductionRadius = 6f;
    public float returnSpeedMultiplier = 1.2f;

    [Header("Targeting")]
    public float searchRadius = 80f;
    public string cowTag = "Cow";
    public float giveUpTimeout = 20f;

    [Header("Health Threshold")]
    [Range(0f, 1f)]
    public float fleeHealthThreshold = 0.3f;

    [Header("Audio")]
    public AudioClip moveLoopClip;
    public float moveLoopVolume = 1f;
    public AudioClip deathClip;
    public float deathVolume = 1f;

    [Header("Events")]
    public UFOControllerEvent OnReachedCow;
    public UFOControllerEvent OnStartAbduction;
    public UFOControllerEvent OnReturnToSpawn;
    public UFOControllerEvent OnUFOFled;
    public UFOControllerEvent OnUFOFinished;
    public UFOControllerEvent OnCowAbducted;

    // internal
    private Transform currentCow;
    private Vector3 spawnPos;
    private float phase;
    private UFO_Abduction abductor;
    private UFO_Health health;
    private AudioSource audioSrc;

    private bool isAbducting = false;
    private bool isReturning = false;
    private bool forcedFlee = false;

    private static HashSet<Transform> reservedCows = new HashSet<Transform>();


    void Awake()
    {
        spawnPos = transform.position;
        phase = Random.Range(0f, Mathf.PI * 2f);

        abductor = GetComponent<UFO_Abduction>();
        health = GetComponent<UFO_Health>();
        audioSrc = GetComponent<AudioSource>();

        health.OnUFODamaged.AddListener(OnDamaged);
        health.OnUFODestroyed.AddListener(OnDestroyed);

        abductor.OnCowAbducted.AddListener(OnCowAbductionComplete);
        abductor.OnAbductionInterrupted.AddListener(OnAbductionInterrupted);
    }

    void Start()
    {
        PlayMoveSFX();
        StartCoroutine(MainLoop());
    }


    // --------------------------------------------------------------
    IEnumerator MainLoop()
    {
        float t = 0f;

        while (!isReturning && !forcedFlee && health.CurrentHealth > 0f)
        {
            // target cow
            if (currentCow == null)
            {
                currentCow = FindFreeCow();
                if (currentCow != null) reservedCows.Add(currentCow);
            }

            if (currentCow != null && !isAbducting)
            {
                Vector3 target = new Vector3(currentCow.position.x, fixedY, currentCow.position.z);

                MoveZigZag(target);

                float dist = Vector3.Distance(
                    new Vector3(target.x, 0, target.z),
                    new Vector3(transform.position.x, 0, transform.position.z)
                );

                if (dist <= abductionRadius)
                {
                    isAbducting = true;
                    OnStartAbduction?.Invoke(this);
                    abductor.StartAbduction(currentCow, this);
                }
            }
            else
            {
                t += Time.deltaTime;
                Hover();
                if (t >= giveUpTimeout && !isAbducting)
                {
                    ForceFleeToSpawnAndSelfDestruct();
                    yield break;
                }
            }

            // flee condition
            if (health.CurrentHealth / health.maxHealth <= fleeHealthThreshold)
            {
                ForceFleeToSpawnAndSelfDestruct();
                yield break;
            }

            yield return null;
        }
    }

    // --------------------------------------------------------------
    void MoveZigZag(Vector3 target)
    {
        Vector3 dir = (target - transform.position).normalized;
        Vector3 flat = new Vector3(dir.x, 0, dir.z).normalized;

        Vector3 right = Vector3.Cross(Vector3.up, flat).normalized;
        float lateral = Mathf.Sin(Time.time * lateralFrequency + phase) * lateralAmplitude;

        Vector3 move = flat * forwardSpeed * Time.deltaTime + right * lateral * Time.deltaTime;
        transform.position += move;
        transform.position = new Vector3(transform.position.x, fixedY, transform.position.z);

        if (move.sqrMagnitude > 0.001f)
        {
            Quaternion look = Quaternion.LookRotation(new Vector3(move.x, 0, move.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, look, rotationSpeed * Time.deltaTime);
        }
    }

    void Hover()
    {
        transform.position = new Vector3(transform.position.x, fixedY, transform.position.z);
    }

    Transform FindFreeCow()
    {
        GameObject[] cows = GameObject.FindGameObjectsWithTag(cowTag);

        Transform best = null;
        float minDist = Mathf.Infinity;

        foreach (GameObject c in cows)
        {
            if (!c) continue;

            Transform t = c.transform;
            if (reservedCows.Contains(t)) continue;

            float dist = Vector3.Distance(transform.position, t.position);
            if (dist < minDist && dist <= searchRadius)
            {
                minDist = dist;
                best = t;
            }
        }
        return best;
    }

    // --------------------------------------------------------------
    public void NotifyCowAbducted()
    {
        if (currentCow != null)
        {
            reservedCows.Remove(currentCow);
            currentCow = null;
        }
        isAbducting = false;

        StartCoroutine(ReturnToSpawn());
    }

    IEnumerator ReturnToSpawn()
    {
        isReturning = true;

        while (Vector3.Distance(transform.position, spawnPos) > 0.5f)
        {
            Vector3 dir = (spawnPos - transform.position).normalized;
            transform.position += dir * forwardSpeed * returnSpeedMultiplier * Time.deltaTime;
            transform.position = new Vector3(transform.position.x, fixedY, transform.position.z);
            yield return null;
        }

        OnUFOFinished?.Invoke(this);
        Destroy(gameObject);
    }


    // --------------------------------------------------------------
    public void ForceFleeToSpawnAndSelfDestruct()
    {
        if (forcedFlee) return;
        forcedFlee = true;

        if (currentCow != null)
        {
            reservedCows.Remove(currentCow);
            currentCow = null;
        }

        OnUFOFled?.Invoke(this);
        StartCoroutine(ReturnToSpawn());
    }


    // --------------------------------------------------------------
    void OnDamaged(float amt)
    {
        if (isAbducting)
        {
            abductor.InterruptAbduction();
            isAbducting = false;
        }
    }

    void OnDestroyed(UFO_Health src)
    {
        if (currentCow != null)
        {
            reservedCows.Remove(currentCow);
            currentCow = null;
        }

        if (audioSrc && deathClip)
        {
            audioSrc.Stop();
            audioSrc.PlayOneShot(deathClip, deathVolume);
        }

        OnUFOFinished?.Invoke(this);
        Destroy(gameObject, 0.05f);
    }

    // --------------------------------------------------------------
    void PlayMoveSFX()
    {
        if (audioSrc && moveLoopClip)
        {
            audioSrc.clip = moveLoopClip;
            audioSrc.loop = true;
            audioSrc.volume = moveLoopVolume;
            audioSrc.Play();
        }
    }

    // --------------------------------------------------------------
    void OnCowAbductionComplete(UFO_Controller u)
    {
        OnCowAbducted?.Invoke(this);
        NotifyCowAbducted();
    }

    void OnAbductionInterrupted(UFO_Controller u)
    {
        isAbducting = false;
    }
}
