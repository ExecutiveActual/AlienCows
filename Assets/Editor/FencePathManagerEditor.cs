using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(FencePathManager))]
public class FencePathManagerEditor : Editor
{
    private FencePathManager manager;
    private const float HandleSize = 0.5f;
    private const float PickSize = 0.7f;

    private int selectedPointIndex = -1;
    private void OnEnable()
    {
        manager = (FencePathManager)target;
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    private void OnSceneGUI(SceneView sceneView)
    {

        if (Selection.activeGameObject != manager.gameObject) return;

        Event guiEvent = Event.current;
        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

        DrawPath();
        ProcessInput(guiEvent);


        sceneView.Repaint();
    }

    private void DrawPath()
    {
        if (manager.pathPoints.Count < 1) return;


        Handles.color = Color.cyan;
        for (int i = 0; i < manager.pathPoints.Count; i++)
        {
            Vector3 point = manager.pathPoints[i];


            float size = HandleUtility.GetHandleSize(point) * HandleSize;
            if (Handles.Button(point, Quaternion.identity, size, PickSize, Handles.SphereHandleCap))
            {
                selectedPointIndex = i;
                Repaint();
            }


            if (i == selectedPointIndex)
            {
                Handles.color = Color.yellow;
            }
            else
            {
                Handles.color = Color.cyan;
            }


            Vector3 newPosition = Handles.PositionHandle(point, Quaternion.identity);
            if (newPosition != point)
            {
                Undo.RecordObject(manager, "Move Path Point");
                manager.pathPoints[i] = newPosition;
                EditorUtility.SetDirty(manager);
            }


            if (i < manager.pathPoints.Count - 1)
            {
                Handles.color = Color.white;
                Handles.DrawLine(point, manager.pathPoints[i + 1]);
            }
        }
    }

    private void ProcessInput(Event guiEvent)
    {
        Ray mouseRay = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition);
        RaycastHit hit;

        // Add a new point (Shift + Left Click)
        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.shift)
        {
            if (Physics.Raycast(mouseRay, out hit, Mathf.Infinity, manager.terrainLayer))
            {
                Undo.RecordObject(manager, "Add Path Point");
                manager.pathPoints.Add(hit.point);
                selectedPointIndex = manager.pathPoints.Count - 1;
                EditorUtility.SetDirty(manager);
            }
            guiEvent.Use();
        }

        else if (guiEvent.type == EventType.MouseDown && guiEvent.button == 1 && guiEvent.shift)
        {
            if (selectedPointIndex != -1 && selectedPointIndex < manager.pathPoints.Count)
            {
                Undo.RecordObject(manager, "Delete Path Point");
                manager.pathPoints.RemoveAt(selectedPointIndex);
                selectedPointIndex = -1;
                EditorUtility.SetDirty(manager);
            }
            guiEvent.Use();
        }

        else if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && !guiEvent.shift)
        {

            if (GUIUtility.hotControl == 0)
            {
                selectedPointIndex = -1;
                Repaint();
            }
            guiEvent.Use();
        }
    }


    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        GUILayout.Space(10);
        if (GUILayout.Button("Generate Fences"))
        {
            manager.GenerateFences();
            EditorUtility.SetDirty(manager);
        }

        if (GUILayout.Button("Clear All Fences"))
        {
            if (EditorUtility.DisplayDialog("Clear Fences", "Are you sure you want to delete all generated fences?", "Yes", "No"))
            {
                manager.ClearFences();
                EditorUtility.SetDirty(manager);
            }
        }

        if (GUILayout.Button("Clear Path Points"))
        {
            if (EditorUtility.DisplayDialog("Clear Path", "Are you sure you want to delete all path points?", "Yes", "No"))
            {
                Undo.RecordObject(manager, "Clear Path Points");
                manager.pathPoints.Clear();
                selectedPointIndex = -1;
                EditorUtility.SetDirty(manager);
            }
        }
    }
}
