using UnityEngine;
using TMPro;
using UnityEngine.Events;
using System.Collections;

public class Cow : MonoBehaviour
{
    [Header("World & Debug")]
    public WorldClock_ worldClock; // Reference to your world clock
    public TMP_Text debugText;

    [Header("Movement Settings")]
    public float wanderRadius = 10f;
    public float moveSpeed = 1.5f;
    public float targetThreshold = 0.5f;

    [Header("State Probabilities (Daytime Only)")]
    [Range(0f, 1f)] public float idleProbability = 0.4f;
    [Range(0f, 1f)] public float grazingProbability = 0.4f;
    [Range(0f, 1f)] public float roamingProbability = 0.2f;

    [Header("Audio Clips")]
    public AudioClip idleSound;
    [Range(0f, 1f)] public float idleVolume = 1f;

    public AudioClip grazingSound;
    [Range(0f, 1f)] public float grazingVolume = 1f;

    public AudioClip sleepSound;
    [Range(0f, 1f)] public float sleepVolume = 1f;

    public AudioClip roamingSound;
    [Range(0f, 1f)] public float roamingVolume = 1f;

    [Header("Audio Settings")]
    public AudioSource cowAudioSource;

    [Header("Unity Events")]
    public UnityEvent OnCowSleep;
    public UnityEvent OnCowGraze;
    public UnityEvent OnCowIdle;
    public UnityEvent OnCowRoam;

    private Vector3 startPoint;
    private Vector3 targetPoint;
    private bool isMoving = false;
    private bool isSleeping = false;
    private bool wasNightBefore = false;

    private enum CowState { Idle, Grazing, Roaming, Sleeping }
    private CowState currentState;

    void Start()
    {
        startPoint = transform.position;
        StartCoroutine(CowDecisionLoop());
    }

    void Update()
    {
        // Handle movement
        if (isMoving && !isSleeping)
            MoveTowardsTarget();

        // Update debug text
        if (debugText != null)
        {
            debugText.text = currentState.ToString();
            if (Camera.main != null)
                debugText.transform.rotation = Quaternion.LookRotation(debugText.transform.position - Camera.main.transform.position);
        }

        // Detect day/night transitions
        if (worldClock != null)
        {
            bool isNightNow = worldClock.IsNight();

            if (isNightNow && !wasNightBefore)
            {
                StopAllCoroutines();
                StartCoroutine(SleepRoutine());
            }
            else if (!isNightNow && wasNightBefore)
            {
                StopAllCoroutines();
                StartCoroutine(CowDecisionLoop());
            }

            wasNightBefore = isNightNow;
        }

        // Stop roaming loop sound if cow stops moving
        if (currentState == CowState.Roaming && cowAudioSource != null)
        {
            if (!isMoving && cowAudioSource.isPlaying && cowAudioSource.loop)
                cowAudioSource.Stop();
        }
    }

    IEnumerator CowDecisionLoop()
    {
        while (true)
        {
            if (worldClock != null && worldClock.IsNight())
            {
                yield return SleepRoutine();
            }
            else
            {
                float rand = Random.value;

                if (rand < idleProbability)
                {
                    SwitchState(CowState.Idle);
                    OnCowIdle?.Invoke();
                    PlaySound(idleSound, idleVolume, false);
                    yield return new WaitForSeconds(Random.Range(4f, 8f));
                }
                else if (rand < idleProbability + grazingProbability)
                {
                    SwitchState(CowState.Grazing);
                    OnCowGraze?.Invoke();
                    PlaySound(grazingSound, grazingVolume, false);
                    yield return new WaitForSeconds(Random.Range(6f, 10f));
                }
                else
                {
                    SwitchState(CowState.Roaming);
                    OnCowRoam?.Invoke();
                    PlaySound(roamingSound, roamingVolume, true); // 🆕 loop = true
                    targetPoint = GetRandomPointInRadius();
                    isMoving = true;
                    yield return new WaitForSeconds(Random.Range(4f, 8f));
                    isMoving = false;
                    cowAudioSource.loop = false; // stop loop after done roaming
                    cowAudioSource.Stop();
                }
            }
        }
    }

    IEnumerator SleepRoutine()
    {
        SwitchState(CowState.Sleeping);
        OnCowSleep?.Invoke();
        PlaySound(sleepSound, sleepVolume, false);
        isSleeping = true;

        // Cow stays asleep until night ends
        while (worldClock != null && worldClock.IsNight())
            yield return null;

        isSleeping = false;
        StartCoroutine(CowDecisionLoop());
    }

    void SwitchState(CowState newState)
    {
        currentState = newState;
        isSleeping = (newState == CowState.Sleeping);
    }

    void PlaySound(AudioClip clip, float volume, bool loop)
    {
        if (cowAudioSource == null) return;
        cowAudioSource.Stop();

        if (clip != null)
        {
            cowAudioSource.clip = clip;
            cowAudioSource.volume = volume;
            cowAudioSource.loop = loop;
            cowAudioSource.Play();
        }
    }

    void MoveTowardsTarget()
    {
        Vector3 direction = (targetPoint - transform.position).normalized;
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            lookRotation *= Quaternion.Euler(0, -90f, 0);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 2f);
        }

        transform.position += transform.right * moveSpeed * Time.deltaTime;

        if (Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z),
                             new Vector3(targetPoint.x, 0, targetPoint.z)) < targetThreshold)
        {
            isMoving = false;
        }
    }

    Vector3 GetRandomPointInRadius()
    {
        Vector2 randomCircle = Random.insideUnitCircle * wanderRadius;
        return new Vector3(startPoint.x + randomCircle.x, startPoint.y, startPoint.z + randomCircle.y);
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(Application.isPlaying ? startPoint : transform.position, wanderRadius);
    }
#endif
}
