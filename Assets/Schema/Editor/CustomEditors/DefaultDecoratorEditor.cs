using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;

[CustomEditor(typeof(Schema.Decorator))]
public class DefaultDecoratorEditor : Editor
{
    private bool shouldShowAborts = false;
    private SerializedProperty decoratorName;
    private SerializedProperty aborts;
    private SerializedProperty conditionalValue;
    void OnEnable()
    {
        if (target != null && serializedObject != null)
        {
            try
            {
                Type targetType = ((Schema.Decorator)target).GetType();
                Type declaringType = targetType.GetMethod("Evaluate").DeclaringType;

                decoratorName = serializedObject.FindProperty("m_Name");

                shouldShowAborts = declaringType.Equals(targetType);

                aborts = serializedObject.FindProperty("abortsType");
                conditionalValue = serializedObject.FindProperty("conditionalValue");
            }
            catch { }
        }
    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        Schema.Decorator decorator = (Schema.Decorator)target;

        EditorGUILayout.PropertyField(decoratorName);

        if (shouldShowAborts)
        {
            EditorGUILayout.PropertyField(aborts);
            EditorGUILayout.PropertyField(conditionalValue);
        }

        serializedObject.ApplyModifiedProperties();
    }
}