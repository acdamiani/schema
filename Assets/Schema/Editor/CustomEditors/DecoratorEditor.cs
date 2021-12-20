using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;

[CustomEditor(typeof(Schema.Runtime.Decorator))]
public class DecoratorEditor : Editor
{
    private bool shouldShowAborts = false;
    private SerializedProperty aborts;
    void OnEnable()
    {
        if (target)
        {
            try
            {
                Type targetType = ((Schema.Runtime.Decorator)target).GetType();
                Type declaringType = targetType.GetMethod("Evaluate").DeclaringType;

                shouldShowAborts = declaringType.Equals(targetType);

                aborts = serializedObject.FindProperty("abortsType");
            }
            catch
            {
                Debug.Log(target.GetType());
            }
        }
    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        Schema.Runtime.Decorator decorator = (Schema.Runtime.Decorator)target;

        if (shouldShowAborts)
            EditorGUILayout.PropertyField(aborts);

        serializedObject.ApplyModifiedProperties();
    }
}