using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ConeCheck)), CanEditMultipleObjects]
public class ConeCheckEditor : Editor
{
    private int tab;
    private SerializedProperty coneType;
    private SerializedProperty halfAngle;
    private SerializedProperty visualize;
    private SerializedProperty tagFilter;
    private SerializedProperty resolution;
    private SerializedProperty rayRange;
    private SerializedProperty offset;
    private SerializedProperty direction;
    private SerializedProperty gameObjectKey;
    private void OnEnable()
    {
        coneType = serializedObject.FindProperty("precisionMode");
        halfAngle = serializedObject.FindProperty("halfAngle");
        visualize = serializedObject.FindProperty("visualize");
        tagFilter = serializedObject.FindProperty("tagFilter");
        resolution = serializedObject.FindProperty("resolution");
        rayRange = serializedObject.FindProperty("rayRange");
        offset = serializedObject.FindProperty("offset");
        direction = serializedObject.FindProperty("coneDirection");
        gameObjectKey = serializedObject.FindProperty("gameObjectKey");
    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUI.BeginChangeCheck();

        tab = GUILayout.Toolbar(coneType.boolValue ? 1 : 0, new string[] { "Efficient", "Precise" });
        coneType.boolValue = tab == 1;

        EditorGUILayout.PropertyField(visualize);

        EditorGUILayout.LabelField("Cone properties", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(tagFilter);
        EditorGUILayout.PropertyField(gameObjectKey);

        EditorGUILayout.PropertyField(halfAngle);
        EditorGUILayout.PropertyField(rayRange);
        EditorGUILayout.PropertyField(offset);
        EditorGUILayout.PropertyField(direction);

        if (coneType.boolValue)
        {
            EditorGUILayout.PropertyField(resolution);
        }

        if (EditorGUI.EndChangeCheck())
            SceneView.RepaintAll();

        serializedObject.ApplyModifiedProperties();
    }
}