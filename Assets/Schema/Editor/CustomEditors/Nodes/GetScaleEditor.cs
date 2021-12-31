using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GetScale)), CanEditMultipleObjects]
public class GetScaleEditor : Editor
{
    SerializedProperty useSelf;
    SerializedProperty gameObjectKey;
    SerializedProperty positionKey;
    void OnEnable()
    {
        useSelf = serializedObject.FindProperty("useSelf");
        gameObjectKey = serializedObject.FindProperty("gameObject");
        positionKey = serializedObject.FindProperty("scaleKey");
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