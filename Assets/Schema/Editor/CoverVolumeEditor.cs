using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(CoverVolume))]
public class CoverVolumeEditor : Editor
{
    private readonly BoxBoundsHandle handle = new();
    private SerializedProperty center;
    private SerializedProperty filter;
    private SerializedProperty size;
    private bool editingCollider => EditMode.editMode == EditMode.SceneViewEditMode.Collider && EditMode.IsOwner(this);

    private void OnEnable()
    {
        size = serializedObject.FindProperty("size");
        center = serializedObject.FindProperty("center");
        filter = serializedObject.FindProperty("filter");
    }

    private void OnSceneGUI()
    {
        if (!editingCollider)
            return;

        CoverVolume vol = (CoverVolume)target;
        using (new Handles.DrawingScope(new Color(1f, 1f, 1f, 1f), vol.transform.localToWorldMatrix))
        {
            handle.center = vol.center;
            handle.size = vol.size;

            EditorGUI.BeginChangeCheck();
            handle.DrawHandle();
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(vol, "Modified Cover Volume");
                Vector3 center = handle.center;
                Vector3 size = handle.size;
                vol.center = center;
                vol.size = size;
                EditorUtility.SetDirty(target);
            }
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        CoverVolume volume = (CoverVolume)target;

        EditMode.DoEditModeInspectorModeButton(EditMode.SceneViewEditMode.Collider, "Edit Volume",
            EditorGUIUtility.IconContent("EditCollider"), GetBounds, this);


        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(filter);
        EditorGUILayout.PropertyField(size);
        EditorGUILayout.PropertyField(center);

        volume.preferredDist = EditorGUILayout.FloatField(
            new GUIContent("Preferred Distance",
                "The distance that the point should be within to be considered \"cover\""), volume.preferredDist);
        volume.preferredDist = Mathf.Clamp(volume.preferredDist, 0.0f, float.MaxValue);

        volume.spacing = EditorGUILayout.Slider("Spacing", volume.spacing, 0.25f, 10f);
        if (EditorGUI.EndChangeCheck()) SceneView.RepaintAll();

        serializedObject.ApplyModifiedProperties();
    }

    private Bounds GetBounds()
    {
        CoverVolume v = (CoverVolume)target;
        return new Bounds(v.center, v.size);
    }

    [DrawGizmo((GizmoType)(-1))]
    private static void OnDrawGizmosSelected(CoverVolume volume, GizmoType gizmoType)
    {
        Transform t = volume.transform;

        Color lastCol = Gizmos.color;
        Matrix4x4 lastMatrix = Gizmos.matrix;

        Gizmos.matrix = volume.transform.localToWorldMatrix;

        Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
        Gizmos.DrawCube(volume.center, volume.size);
        Gizmos.color = new Color32(187, 138, 240, 210);
        Gizmos.DrawWireCube(volume.center, volume.size);

        Gizmos.color = lastCol;
        Gizmos.matrix = lastMatrix;

        Vector3[] p = volume.GeneratePoints();

        foreach (Vector3 v in p) Gizmos.DrawCube(v, Vector3.one * .1f);
    }
}