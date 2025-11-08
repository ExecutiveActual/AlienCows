using UnityEngine;
using System.Collections;

public class CowIntelegence_ : MonoBehaviour
{
    [Header("Cow Movement Settings")]
    [Tooltip("How far the cow can wander from its starting point")]
    public float wanderRadius = 10f;

    [Tooltip("Cow walking speed (m/s)")]
    public float moveSpeed = 1.5f;

    [Tooltip("How close the cow must be to its target to choose a new one")]
    public float targetThreshold = 0.5f;

    [Header("Cycle Timings (seconds)")]
    public float grazingTime = 8f;
    public float idleTime = 5f;
    public float sleepTime = 15f;

    [Header("Behavior Probabilities")]
    [Range(0f, 1f)] public float moveProbability = 0.85f; // 85% move, rest graze/idle/sleep

    private Vector3 startPoint;
    private Vector3 targetPoint;
    private bool isMoving = false;
    private bool isSleeping = false;

    private enum CowState { Moving, Grazing, Idle, Sleeping }
    private CowState currentState;

    void Start()
    {
        startPoint = transform.position;
        StartCoroutine(CowBehaviorCycle());
    }

    void Update()
    {
        if (isMoving && !isSleeping)
        {
            MoveTowardsTarget();
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

    IEnumerator CowBehaviorCycle()
    {
        while (true)
        {
            
            float randomChance = Random.value;
            if (randomChance <= moveProbability)
            {
                currentState = CowState.Moving;
                isMoving = true;
                targetPoint = GetRandomPointInRadius();

                float moveDuration = Random.Range(5f, 10f);
                yield return new WaitForSeconds(moveDuration);
                isMoving = false;
            }

            
            currentState = CowState.Grazing;
            yield return StartCoroutine(Graze());

            
            currentState = CowState.Idle;
            yield return StartCoroutine(Idle());

            
            currentState = CowState.Moving;
            isMoving = true;
            targetPoint = GetRandomPointInRadius();
            float moveDuration2 = Random.Range(4f, 8f);
            yield return new WaitForSeconds(moveDuration2);
            isMoving = false;

            
            currentState = CowState.Sleeping;
            yield return StartCoroutine(Sleep());
        }
    }

    Vector3 GetRandomPointInRadius()
    {
        Vector2 randomCircle = Random.insideUnitCircle * wanderRadius;
        Vector3 randomPos = new Vector3(startPoint.x + randomCircle.x, startPoint.y, startPoint.z + randomCircle.y);
        return randomPos;
    }

    IEnumerator Graze()
    {
        yield return new WaitForSeconds(Random.Range(grazingTime - 2f, grazingTime + 2f));
    }

    IEnumerator Idle()
    {
        yield return new WaitForSeconds(Random.Range(idleTime - 2f, idleTime + 2f));
    }

    IEnumerator Sleep()
    {
        isSleeping = true;
        yield return new WaitForSeconds(Random.Range(sleepTime - 5f, sleepTime + 5f));
        isSleeping = false;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(Application.isPlaying ? startPoint : transform.position, wanderRadius);
    }
#endif
}
