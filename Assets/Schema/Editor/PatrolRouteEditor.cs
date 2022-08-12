using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

[CustomEditor(typeof(PatrolRoute))]
public class PatrolRouteEditor : Editor
{
    private bool isDragging;

    private void OnSceneGUI()
    {
        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));

        PatrolRoute route = (PatrolRoute)target;

        if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                Vector3 position = hit.point;

                if (isDragging)
                    route.points[route.selected] = position;

                int i = 0;
                foreach (Vector3 v in route.points)
                {
                    if (Vector3.Distance(v, position) <= HandleUtility.GetHandleSize(position) * 0.5f)
                    {
                        isDragging = true;
                        route.selected = i;
                    }

                    i++;
                }

                if (!isDragging)
                {
                    NavMeshHit navHit;

                    if (NavMesh.SamplePosition(position, out navHit, 1.0f, NavMesh.AllAreas))
                    {
                        Undo.RecordObject(route, "Add Point");
                        route.selected = route.points.Count;
                        isDragging = true;
                        route.points.Add(navHit.position);
                        Event.current.Use();
                    }
                }
            }
        }
        else if (Event.current.type == EventType.MouseUp && Event.current.button == 0)
        {
            isDragging = false;
        }
        else if (Event.current.type == EventType.KeyDown)
        {
            switch (Event.current.keyCode)
            {
                case KeyCode.X:
                    if (route.selected > -1)
                    {
                        Undo.RecordObject(route, "Remove Point");
                        route.points.RemoveAt(route.selected);
                        route.selected = -1;
                        Event.current.Use();
                    }

                    break;
            }
        }

        if (isDragging)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                Vector3 position = hit.point;
                NavMeshHit navMeshHit;

                if (NavMesh.SamplePosition(position, out navMeshHit, 1.0f, -1))
                    route.points[route.selected] = navMeshHit.position;
            }
        }

        SceneView.RepaintAll();
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Clear Path"))
            ((PatrolRoute)target).points.Clear();
    }
}