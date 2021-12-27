using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GetRotation)), CanEditMultipleObjects]
public class GetRotationEditor : Editor
{
    SerializedProperty useSelf;
    SerializedProperty eulerAngles;
    SerializedProperty gameObjectKey;
    SerializedProperty eulerKey;
    SerializedProperty quaternionKey;
    void OnEnable()
    {
        useSelf = serializedObject.FindProperty("useSelf");
        eulerAngles = serializedObject.FindProperty("eulerAngles");
        gameObjectKey = serializedObject.FindProperty("gameObject");
        eulerKey = serializedObject.FindProperty("eulerKey");
        quaternionKey = serializedObject.FindProperty("quaternionKey");
    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(useSelf);

        if (!useSelf.boolValue)
            EditorGUILayout.PropertyField(gameObjectKey);

        EditorGUILayout.PropertyField(eulerAngles);

        if (eulerAngles.boolValue)
            EditorGUILayout.PropertyField(eulerKey);
        else
            EditorGUILayout.PropertyField(quaternionKey);

        serializedObject.ApplyModifiedProperties();
    }
}