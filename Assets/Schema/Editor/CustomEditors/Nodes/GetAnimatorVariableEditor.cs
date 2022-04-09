using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GetAnimatorVariable))]
public class GetAnimatorVariableEditor : Editor
{
    SerializedProperty type;
    SerializedProperty parameterName;
    SerializedProperty floatValue;
    SerializedProperty intValue;
    SerializedProperty boolValue;

    void OnEnable()
    {
        type = serializedObject.FindProperty("type");
        parameterName = serializedObject.FindProperty("parameterName");
        floatValue = serializedObject.FindProperty("floatValue");
        intValue = serializedObject.FindProperty("intValue");
        boolValue = serializedObject.FindProperty("boolValue");
    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(type);
        EditorGUILayout.PropertyField(parameterName);

        switch (type.enumValueIndex)
        {
            case 0:
                EditorGUILayout.PropertyField(floatValue, new GUIContent("Selector"));
                break;
            case 1:
                EditorGUILayout.PropertyField(intValue, new GUIContent("Selector"));
                break;
            case 2:
                EditorGUILayout.PropertyField(boolValue, new GUIContent("Selector"));
                break;
        }

        serializedObject.ApplyModifiedProperties();
    }
}