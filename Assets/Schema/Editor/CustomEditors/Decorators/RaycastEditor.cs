using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Raycast)), CanEditMultipleObjects]
public class RaycastEditor : Editor
{
    private SerializedProperty visualize;
    private SerializedProperty offset;
    private SerializedProperty direction;
    private SerializedProperty point;
    private SerializedProperty tagFilter;
    private SerializedProperty maxDistance;
    private void OnEnable()
    {
        visualize = serializedObject.FindProperty("visualize");
        offset = serializedObject.FindProperty("offset");
        direction = serializedObject.FindProperty("direction");
        point = serializedObject.FindProperty("point");
        tagFilter = serializedObject.FindProperty("tagFilter");
        maxDistance = serializedObject.FindProperty("maxDistance");
    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        Raycast raycast = (Raycast)target;

        raycast.type = (Raycast.RaycastType)GUILayout.Toolbar((int)raycast.type, new string[] { "Absolute", "Dynamic" });

        EditorGUILayout.PropertyField(visualize);
        EditorGUILayout.PropertyField(tagFilter);

        if (raycast.type == Raycast.RaycastType.Absolute)
        {
            EditorGUILayout.PropertyField(offset);
            EditorGUILayout.PropertyField(direction);
            EditorGUILayout.PropertyField(maxDistance);
        }
        else
        {
            EditorGUILayout.PropertyField(point);
        }

        serializedObject.ApplyModifiedProperties();
    }
}