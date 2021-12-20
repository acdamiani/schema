using UnityEngine;
using System.Collections.Generic;
using System;

namespace Schema.Runtime
{
    [CreateAssetMenu(menuName = "Schema/Behavior Tree")]
    [Serializable]
    public class Graph : ScriptableObject
    {
        public Root root;
        public List<Node> nodes;
        public Blackboard blackboard;
        public void RemoveNode(Node node)
        {
            nodes.Remove(node);
            if (node.parent != null) node.parent.children.Remove(node);
            node.children.ForEach(child => child.parent = null);
            if (Application.isPlaying) Destroy(node);
        }

        private void OnDestroy()
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                Node node = nodes[i];

                for (int j = 0; j < node.decorators.Count; j++)
                {
                    Decorator decorator = node.decorators[j];
                    DestroyImmediate(decorator);
                }

                node.decorators.Clear();

                DestroyImmediate(node);
            }

            for (int i = 0; i < blackboard.entries.Count; i++)
            {
                BlackboardEntry entry = blackboard.entries[i];
                DestroyImmediate(entry);
            }

            DestroyImmediate(blackboard);

            nodes.Clear();
        }

        //These must be properties so it is not included in Undo
#if UNITY_EDITOR
        public float zoom = 1f;
        public Vector2 pan;
#endif
    }
    public struct Error
    {
        public enum Severity
        {
            Info,
            Warning,
            Error
        }
        public string message;
        public Severity severity;
        public Error(string message, Severity severity)
        {
            this.message = message;
            this.severity = severity;
        }
    }
}