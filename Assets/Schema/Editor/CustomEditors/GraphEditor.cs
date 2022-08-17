using Schema;
using UnityEditor;
using UnityEngine;

namespace SchemaEditor.Editors
{
    [CustomEditor(typeof(Graph))]
    public class GraphEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            Graph active = (Graph)target;

            int count = active.nodes == null ? 0 : active.nodes.Length;

            GUILayout.Label(count + (count == 1 ? " node" : " nodes"));
            GUILayout.Space(10);

            if (GUILayout.Button("Open in Editor"))
                AssetDatabase.OpenAsset(active.GetInstanceID());
        }
    }
}