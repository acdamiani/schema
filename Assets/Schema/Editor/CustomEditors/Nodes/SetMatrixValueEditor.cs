using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SetMatrixValue))]
public class SetMatrixValueEditor : Editor
{
    float padding = 10f;
    SerializedProperty[,] matrix = new SerializedProperty[4, 4];
    SerializedProperty t;
    void OnEnable()
    {
        for (int x = 0; x < 4; x++)
        {
            for (int y = 0; y < 4; y++)
            {
                matrix[y, x] = serializedObject.FindProperty($"m{x}{y}");
            }
        }

        t = serializedObject.FindProperty("target");
    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        Rect reserved = GUILayoutUtility.GetRect(100f, Single.MaxValue, 0f, 16f * 4 + padding * 3 + 5f);

        for (int y = 0; y < 4; y++)
        {
            for (int x = 0; x < 4; x++)
            {
                Rect r = new Rect(
                    reserved.x + reserved.width / 4f * x + padding / 2f,
                    reserved.y + 16f * y + padding * y,
                    (reserved.width - padding * 3) / 4f,
                    16f
                );

                EditorGUI.PropertyField(r, matrix[x, y], GUIContent.none, false);
            }
        }

        EditorGUILayout.PropertyField(t);

        serializedObject.ApplyModifiedProperties();
    }
}