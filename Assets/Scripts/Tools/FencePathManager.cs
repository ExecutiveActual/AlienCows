using UnityEngine;
using System.Collections.Generic;

public class FencePathManager : MonoBehaviour
{
    public GameObject fencePrefab;
    public float fenceSegmentLength = 2f;
    public float yOffset = 0f;
    public float heightRandomness = 0.0f;
    public float customRotationY = 0f;
    public LayerMask terrainLayer;

    public List<Vector3> pathPoints = new List<Vector3>();

    public void GenerateFences()
    {
        ClearFences();

        if (pathPoints.Count < 2 || fencePrefab == null)
        {
            Debug.LogWarning("Need at least 2 path points and a fence prefab to generate fences.");
            return;
        }

        float totalPathLength = GetPathLength();
        float distancePlaced = 0;

        while (distancePlaced < totalPathLength)
        {
            Vector3 currentPathPosition = GetPointOnPath(distancePlaced);
            Vector3 nextPathPosition = GetPointOnPath(distancePlaced + 0.01f);

            Vector3 direction = (nextPathPosition - currentPathPosition).normalized;
            if (direction == Vector3.zero) direction = Vector3.forward;

            Quaternion rotation = Quaternion.LookRotation(direction);

            // Apply custom Y-axis rotation
            rotation *= Quaternion.Euler(0, customRotationY, 0); // Apply additional rotation around Y

            Vector3 finalPlacementPosition = currentPathPosition;
            RaycastHit hit;
            if (Physics.Raycast(currentPathPosition + Vector3.up * 100, Vector3.down, out hit, Mathf.Infinity, terrainLayer))
            {
                float randomHeightAdjustment = Random.Range(-heightRandomness, heightRandomness);
                finalPlacementPosition.y = hit.point.y + yOffset + randomHeightAdjustment;
            }
            else
            {
                float randomHeightAdjustment = Random.Range(-heightRandomness, heightRandomness);
                finalPlacementPosition.y += yOffset + randomHeightAdjustment;
            }

            GameObject newFence = Instantiate(fencePrefab, finalPlacementPosition, rotation);
            newFence.transform.parent = transform;
            newFence.name = fencePrefab.name + "_" + (transform.childCount -1);

            distancePlaced += fenceSegmentLength;
        }
    }

    public void ClearFences()
    {
        List<GameObject> childrenToDestroy = new List<GameObject>();
        foreach (Transform child in transform)
        {
            childrenToDestroy.Add(child.gameObject);
        }

        foreach (GameObject child in childrenToDestroy)
        {
            DestroyImmediate(child);
        }
    }

    public float GetPathLength()
    {
        float length = 0;
        for (int i = 0; i < pathPoints.Count - 1; i++)
        {
            length += Vector3.Distance(pathPoints[i], pathPoints[i + 1]);
        }
        return length;
    }

    public Vector3 GetPointOnPath(float distance)
    {
        if (pathPoints.Count < 2) return Vector3.zero;

        float currentLength = 0;
        for (int i = 0; i < pathPoints.Count - 1; i++)
        {
            float segmentLength = Vector3.Distance(pathPoints[i], pathPoints[i + 1]);
            if (distance <= currentLength + segmentLength)
            {
                float t = (distance - currentLength) / segmentLength;
                return Vector3.Lerp(pathPoints[i], pathPoints[i + 1], t);
            }
            currentLength += segmentLength;
        }
        return pathPoints[pathPoints.Count - 1];
    }
}
