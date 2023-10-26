using Schema.Builtin.Nodes;
using UnityEditor;

namespace SchemaEditor.Editors.Nodes
{
    [CustomEditor(typeof(CancelInvoke)), CanEditMultipleObjects]
    public class CancelInvokeEditor : Editor
    {
        private SerializedProperty cancelAll;
        private SerializedProperty methodName;
        private SerializedProperty monoBehavior;

        private void OnEnable()
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
}