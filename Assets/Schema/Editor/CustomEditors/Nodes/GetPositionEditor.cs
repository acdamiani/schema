using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GetPosition)), CanEditMultipleObjects]
public class GetPositionEditor : Editor
{
    SerializedProperty useSelf;
    SerializedProperty gameObjectKey;
    SerializedProperty positionKey;
    void OnEnable()
    {
        useSelf = serializedObject.FindProperty("useSelf");
        gameObjectKey = serializedObject.FindProperty("gameObject");
        positionKey = serializedObject.FindProperty("positionKey");
    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(useSelf);

        if (!useSelf.boolValue)
            EditorGUILayout.PropertyField(gameObjectKey);

        EditorGUILayout.PropertyField(positionKey);

        serializedObject.ApplyModifiedProperties();
    }
}