using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class PatrolRoute : MonoBehaviour
{
    public List<Vector3> points = new List<Vector3>();
    [HideInInspector] public int selected;
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (points == null) return;

        for (int i = 0; i < points.Count; i++)
        {
            Vector3 point = points[i];

            if (points.Count > 1)
            {
                int lastIndex = i - 1 < 0 ? points.Count - 1 : i - 1;

                Vector3 last = points[lastIndex];
                NavMeshPath path = new NavMeshPath();

                NavMesh.CalculatePath(last, point, NavMesh.AllAreas, path);

                if (path.corners.Length == 0)
                    return;

                Vector3[] corners = path.corners;

                points[i] = corners[corners.Length - 1];
                points[lastIndex] = corners[0];

                for (int j = 0; j < corners.Length; j++)
                {
                    if (j - 1 >= 0)
                        Handles.DrawBezier(corners[j - 1], corners[j], corners[j - 1], corners[j], Color.white, null, 5f);
                }
            }

            if (selected == i)
                Gizmos.color = Color.green;
            else if (i == 0)
                Gizmos.color = Color.cyan;
            else
                Gizmos.color = Color.red;

            Gizmos.DrawWireSphere(point, HandleUtility.GetHandleSize(point) * 0.5f);
        }
    }
#endif
}
