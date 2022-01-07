using UnityEngine;
using UnityEditor;
using System;
using Schema.Utilities;

[CustomEditor(typeof(BlackboardCompare))]
public class BlackboardCompareEditor : Editor
{
    SerializedProperty entryOne;
    SerializedProperty entryTwo;
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        BlackboardCompare obj = (BlackboardCompare)target;
        entryOne = serializedObject.FindProperty("entryOne");
        entryTwo = serializedObject.FindProperty("entryTwo");

        EditorGUILayout.PropertyField(serializedObject.FindProperty("entryOne"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("entryTwo"), true);

        Type t1 = Type.GetType(obj.entryOne.GetEditorEntry()?.typeString);
        Type t2 = Type.GetType(obj.entryTwo.GetEditorEntry()?.typeString);

        if (t1 == null || t2 == null)
            return;

        if ((t1.IsNumeric() && t2.IsNumeric()) && (t1.IsDecimal() || t2.IsDecimal()))
            obj.epsilon = Mathf.Clamp(EditorGUILayout.FloatField("Epsilon", obj.epsilon), 0f, float.MaxValue);

        obj.comparisonTypes = (BlackboardCompare.ComparisonType)EditorGUILayout.EnumPopup(
            new GUIContent("Comparison Type"), obj.comparisonTypes, (e) => ComparisonIsValid(t1, t2, (BlackboardCompare.ComparisonType)e), false
        );

        obj.comparisonTypes = ComparisonIsValid(t1, t2, obj.comparisonTypes) ? obj.comparisonTypes : BlackboardCompare.ComparisonType.Equal;

        serializedObject.ApplyModifiedProperties();
    }
    bool ComparisonIsValid(Type type1, Type type2, BlackboardCompare.ComparisonType comparisonType)
    {
        bool isNumeric = type1.IsNumeric() && type2.IsNumeric();

        if (isNumeric) return true;

        return (comparisonType &
            (
                BlackboardCompare.ComparisonType.Equal |
                BlackboardCompare.ComparisonType.NotEqual
            )) != 0;
    }
}