using UnityEditor;
using UnityEngine;

namespace SchemaEditor.Editors.Nodes
{
    [CustomEditor(typeof(SetMatrixValue))]
    public class SetMatrixValueEditor : Editor
    {
        private readonly SerializedProperty[,] matrix = new SerializedProperty[4, 4];
        private readonly float padding = 10f;
        private SerializedProperty t;

        private void OnEnable()
        {
            for (int x = 0; x < 4; x++)
            for (int y = 0; y < 4; y++)
                matrix[y, x] = serializedObject.FindProperty($"m{x}{y}");

            t = serializedObject.FindProperty("target");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(t);

            GUILayout.Box("", GUILayout.Height(20 * 4 + padding * 5), GUILayout.ExpandWidth(true));

            Rect reserved = GUILayoutUtility.GetLastRect();

            for (int y = 0; y < 4; y++)
            for (int x = 0; x < 4; x++)
            {
                Rect r = new Rect(
                    reserved.x + reserved.width / 4f * x + padding / 2f,
                    reserved.y + 20 * y + padding * y + padding,
                    (reserved.width - padding * 3) / 4f,
                    20
                );

                EditorGUI.PropertyField(r, matrix[x, y], GUIContent.none, false);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}