using System.Collections.Generic;
using System.Linq;
using Schema;
using Schema.Builtin.Nodes;
using UnityEditor;
using UnityEngine;

namespace SchemaEditor.Editors.Nodes
{
    [CustomEditor(typeof(SelectRandomWeighted)), CanEditMultipleObjects]
    public class SelectRandomWeightedEditor : Editor
    {
        private SerializedProperty weights;

        private void OnEnable()
        {
            weights = serializedObject.FindProperty("weights");
        }

        public override void OnInspectorGUI()
        {
            SelectRandomWeighted node = (SelectRandomWeighted)target;

            node.weights ??= new SerializableDictionary<string, int>();

            Dictionary<string, int> d = new Dictionary<string, int>(node.weights);

            foreach (KeyValuePair<string, int> kvp in d)
                if (!node.children.Any(x => x.uID.Equals(kvp.Key)))
                    node.weights.Remove(kvp.Key);

            for (int i = 0; i < node.children.Length; i++)
            {
                Node child = node.children[i];

                EditorGUILayout.LabelField($"{i + 1} {child.name}");

                node.weights[child.uID] =
                    Mathf.Clamp(
                        EditorGUILayout.IntField("Weight",
                            node.weights.ContainsKey(child.uID) ? node.weights[child.uID] : 1), 0, int.MaxValue);

                EditorGUI.BeginChangeCheck();

                long totalWeights = node.weights.Select(x => (long)x.Value).Sum();
                float percentage = node.weights[child.uID] / (totalWeights > 0f ? totalWeights : 1f) * 100f;

                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.Slider("Percentage", Mathf.Round(percentage * 100f) / 100f, 0f, 100f);
                EditorGUI.EndDisabledGroup();
            }
        }
    }
}