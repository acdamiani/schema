using Schema.Builtin.Nodes;
using UnityEditor;

namespace SchemaEditor.Editors.Nodes
{
    [CustomEditor(typeof(PlaySound)), CanEditMultipleObjects]
    public class PlaySoundEditor : Editor
    {
        private SerializedProperty clip;
        private SerializedProperty isOneShot;
        private SerializedProperty volume;
        private SerializedProperty waitForCompletion;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            isOneShot = serializedObject.FindProperty("isOneShot");
            clip = serializedObject.FindProperty("clip");
            volume = serializedObject.FindProperty("volume");
            waitForCompletion = serializedObject.FindProperty("waitForCompletion");

            EditorGUILayout.PropertyField(isOneShot);

            if (isOneShot.boolValue)
            {
                EditorGUILayout.PropertyField(clip);
                EditorGUILayout.PropertyField(volume);
            }

            EditorGUILayout.PropertyField(waitForCompletion);

            serializedObject.ApplyModifiedProperties();
        }
    }
}