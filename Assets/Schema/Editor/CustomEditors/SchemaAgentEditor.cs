using Schema;
using UnityEditor;
using UnityEngine;

namespace SchemaEditor.Editors
{
    [CustomEditor(typeof(SchemaAgent)), CanEditMultipleObjects]
    public class SchemaAgentEditor : Editor
    {
        [SerializeField] private bool optionsFoldout;
        private SerializedProperty description;
        private SerializedProperty graphTarget;
        private SerializedProperty maxStepsPerTick;
        private SerializedProperty resetBlackboardOnRestart;
        private SerializedProperty restartWhenComplete;
        private SerializedProperty treePauseTime;

        private void OnEnable()
        {
            graphTarget = serializedObject.FindProperty("m_target");
            description = serializedObject.FindProperty("m_agentDescription");
            restartWhenComplete = serializedObject.FindProperty("m_restartWhenComplete");
            maxStepsPerTick = serializedObject.FindProperty("m_maxStepsPerTick");
            resetBlackboardOnRestart = serializedObject.FindProperty("m_resetBlackboardOnRestart");
            treePauseTime = serializedObject.FindProperty("m_treePauseTime");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.PropertyField(graphTarget);

            EditorGUI.BeginDisabledGroup(graphTarget.objectReferenceValue == null);
            if (GUILayout.Button("Open", GUILayout.ExpandWidth(false)))
                AssetDatabase.OpenAsset(graphTarget.objectReferenceValue);
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(description);

            optionsFoldout = EditorGUILayout.Foldout(optionsFoldout, "Options", true);

            if (optionsFoldout)
            {
                EditorGUILayout.PropertyField(maxStepsPerTick);
                EditorGUILayout.PropertyField(restartWhenComplete);
                EditorGUI.BeginDisabledGroup(!restartWhenComplete.boolValue);
                EditorGUILayout.PropertyField(treePauseTime);
                EditorGUILayout.PropertyField(resetBlackboardOnRestart);
                EditorGUI.EndDisabledGroup();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}