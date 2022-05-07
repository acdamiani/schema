using UnityEngine;
using Schema.Builtin.Nodes;
using System.Collections.Generic;
using UnityEditor;

[CustomEditor(typeof(GetVectorComponents)), CanEditMultipleObjects]
public class GetVectorComponentsEditor : Editor
{
    SerializedProperty vector;
    Dictionary<UnityEngine.Object, GetVectorComponents> vectors = new Dictionary<UnityEngine.Object, GetVectorComponents>();
    SerializedProperty x;
    SerializedProperty y;
    SerializedProperty z;
    SerializedProperty w;
    void OnEnable()
    {
        x = serializedObject.FindProperty("x");
        y = serializedObject.FindProperty("y");
        z = serializedObject.FindProperty("z");
        w = serializedObject.FindProperty("z");
        vector = serializedObject.FindProperty("vector");

        for (int i = 0; i < targets.Length; i++)
            vectors[targets[i]] = ((GetVectorComponents)targets[i]);
    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(vector);

        EditorGUILayout.PropertyField(x);
        EditorGUILayout.PropertyField(y);

        if (vectors[target]?.vector.entryType == typeof(Vector3))
        {
            EditorGUILayout.PropertyField(z);
        }
        else if (vectors[target]?.vector.entryType == typeof(Vector4))
        {
            EditorGUILayout.PropertyField(w);
            EditorGUILayout.PropertyField(z);
        }

        serializedObject.ApplyModifiedProperties();
    }
}