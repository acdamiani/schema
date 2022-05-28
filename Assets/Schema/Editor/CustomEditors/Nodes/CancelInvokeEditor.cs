
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Schema.Builtin.Nodes.CancelInvoke)), CanEditMultipleObjects]
public class CancelInvokeEditor : Editor
{
    SerializedProperty monoBehavior;
    SerializedProperty cancelAll;
    SerializedProperty methodName;
    void OnEnable()
    {
        monoBehavior = serializedObject.FindProperty("monoBehaviour");
        cancelAll = serializedObject.FindProperty("cancelAll");
        methodName = serializedObject.FindProperty("methodName");
    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(monoBehavior);
        EditorGUILayout.PropertyField(cancelAll);

        if (!cancelAll.boolValue)
            EditorGUILayout.PropertyField(methodName);

        serializedObject.ApplyModifiedProperties();
    }
}