using UnityEngine;
using TMPro;
using UnityEngine.Events;
using System.Collections;

public class Cow : MonoBehaviour
{
    [Header("Debug")]
    public TMP_Text debugText;

    [Header("Movement Settings")]
    public float wanderRadius = 10f;
    public float moveSpeed = 1.5f;
    public float targetThreshold = 0.5f;

    [Header("State Probabilities")]
    [Range(0f, 1f)] public float idleProbability = 0.25f;
    [Range(0f, 1f)] public float grazingProbability = 0.25f;
    [Range(0f, 1f)] public float roamingProbability = 0.25f;
    [Range(0f, 1f)] public float sleepingProbability = 0.25f;

    [Header("Unity Events")]
    public UnityEvent OnCowSleep;
    public UnityEvent OnCowGraze;
    public UnityEvent OnCowIdle;
    public UnityEvent OnCowRoam;

    private Vector3 startPoint;
    private Vector3 targetPoint;
    private bool isMoving = false;
    private bool isSleeping = false;

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
    }

    IEnumerator CowDecisionLoop()
    {
        while (true)
        {
            float total = idleProbability + grazingProbability + roamingProbability + sleepingProbability;
            float rand = Random.value * total;

            if (rand < idleProbability)
            {
                SwitchState(CowState.Idle);
                OnCowIdle?.Invoke();
                yield return new WaitForSeconds(Random.Range(4f, 8f));
            }
            else if (rand < idleProbability + grazingProbability)
            {
                SwitchState(CowState.Grazing);
                OnCowGraze?.Invoke();
                yield return new WaitForSeconds(Random.Range(6f, 10f));
            }
            else if (rand < idleProbability + grazingProbability + roamingProbability)
            {
                SwitchState(CowState.Roaming);
                OnCowRoam?.Invoke();
                targetPoint = GetRandomPointInRadius();
                isMoving = true;
                yield return new WaitForSeconds(Random.Range(4f, 8f));
                isMoving = false;
            }
            else
            {
                SwitchState(CowState.Sleeping);
                OnCowSleep?.Invoke();
                isSleeping = true;
                yield return new WaitForSeconds(Random.Range(8f, 15f));
                isSleeping = false;
            }
        }
    }

    void SwitchState(CowState newState)
    {
        currentState = newState;
        isSleeping = (newState == CowState.Sleeping);
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
