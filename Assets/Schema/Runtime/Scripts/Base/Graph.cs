using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Schema
{
    [CreateAssetMenu(menuName = "Schema/Behavior Tree", order = 84)]
    [Serializable]
    public class Graph : ScriptableObject
    {
        public Root root;
        public List<Node> nodes = new List<Node>();
        public Blackboard blackboard;
#if UNITY_EDITOR
        public float zoom = 1f;
        public Vector2 pan;
#endif
        public void Initialize()
        {
            if (blackboard == null)
            {
                blackboard = ScriptableObject.CreateInstance<Blackboard>();
                blackboard.hideFlags = HideFlags.HideInHierarchy;

                string path = AssetDatabase.GetAssetPath(this);

                if (!String.IsNullOrEmpty(path))
                    AssetDatabase.AddObjectToAsset(blackboard, path);
            }

            if (root == null)
                root = AddNode<Root>(Vector2.zero);

            TraverseTree();
        }
        /// <summary>
        /// Add a node to the tree
        /// </summary>
        /// <param name="nodeType">Type of node to add</param>
        /// <param name="position">Position of the node within the graph</param>
        /// <param name="undo">Whether to register this operation to undo</param>
        /// <returns>The node created in the tree</returns>
        /// <exception cref="ArgumentException">Thrown when nodeType does not inherit from Node</exception>
        public Node AddNode(Type nodeType, Vector2 position, bool undo = true)
        {
            if (!typeof(Node).IsAssignableFrom(nodeType))
                throw new ArgumentException("nodeType does not inherit from type Node");

            Node node = (Node)ScriptableObject.CreateInstance(nodeType);
            node.hideFlags = HideFlags.HideInHierarchy;
            node.position = position;
            node.graph = this;

            string path = AssetDatabase.GetAssetPath(this);

            if (!String.IsNullOrEmpty(path))
            {
                AssetDatabase.AddObjectToAsset(node, path);
            }

            if (undo)
            {
                Undo.RegisterCreatedObjectUndo(node, "Node Created");
                Undo.RegisterCompleteObjectUndo(this, "Node Added");
            }

            nodes.Add(node);

            return node;
        }
        /// <summary>
        /// Add a node to the tree
        /// </summary>
        /// <typeparam name="T">Type of node to add. Must inherit from Node</typeparam>
        /// <param name="position">Position of the node within the graph</param>
        /// <param name="undo">Whether to register this operation to undo</param>
        /// <returns>The node created in the tree</returns>
        public T AddNode<T>(Vector2 position, bool undo = true) where T : Node
        {
            return (T)AddNode(typeof(T), position);
        }
        /// <summary>
        /// Remove multiple nodes from the tree
        /// </summary>
        /// <param name="nodes">List to remove</param>
        public void DeleteNodes(IEnumerable<Node> nodes)
        {
            IEnumerable<Node> nodesWithoutRoot = nodes.Where(node => node.GetType() != typeof(Root));

            Undo.IncrementCurrentGroup();
            int groupIndex = Undo.GetCurrentGroup();

            Undo.RegisterCompleteObjectUndo(this, "Delete Nodes");
            this.nodes = this.nodes.Except(nodesWithoutRoot).ToList();

            foreach (Node node in nodesWithoutRoot)
            {
                node.parent?.RemoveConnection(node, actionName: "");

                foreach (Node child in node.children)
                    node.RemoveConnection(child, actionName: "");

                foreach (Decorator decorator in node.decorators)
                    node.RemoveDecorator(decorator, actionName: "");

                Undo.DestroyObjectImmediate(node);
            }

            Undo.SetCurrentGroupName("Delete Nodes");
            Undo.CollapseUndoOperations(groupIndex);
        }
        /// <summary>
        /// Remove all connections for multiple nodes
        /// </summary>
        /// <param name="nodes">List to break</param>
        public void BreakConnections(IEnumerable<Node> nodes)
        {
            Undo.IncrementCurrentGroup();
            int groupIndex = Undo.GetCurrentGroup();

            foreach (Node node in nodes)
            {
                node.parent?.RemoveConnection(node, actionName: "");

                foreach (Node child in node.children)
                    node.RemoveConnection(child, actionName: "");
            }

            Undo.SetCurrentGroupName("Break Connections");
            Undo.CollapseUndoOperations(groupIndex);
        }
        /// <summary>
        /// Add a decorator to multiple nodes
        /// </summary>
        /// <param name="nodes">List of nodes to add decorators to</param>
        /// <param name="decoratorType">Type of decorator to add. Must inherit from type <see cref="Schema.Decorator">Decorator</see>.</param>
        /// <returns>List of created decorators</returns>
        public IEnumerable<Decorator> AddDecorators(IEnumerable<Node> nodes, Type decoratorType)
        {
            Undo.IncrementCurrentGroup();
            int groupIndex = Undo.GetCurrentGroup();

            List<Decorator> decorators = new List<Decorator>();

            foreach (Node node in nodes)
            {
                Decorator decorator = node.AddDecorator(decoratorType);
                decorators.Add(decorator);
            }

            Undo.SetCurrentGroupName("Add Decorators");
            Undo.CollapseUndoOperations(groupIndex);

            return decorators;
        }
        /// <summary>
        /// Recalculate priorities for all nodes
        /// </summary>
        public void TraverseTree()
        {
            foreach (Node node in nodes) node.priority = 0;

            TraverseSubtree(root, 1);
        }
        private int TraverseSubtree(Node node, int i)
        {
            node.priority = i;
            int children = 0;
            foreach (Node child in node.children)
            {
                Debug.Log(child);
                int j = TraverseSubtree(child, i + 1);
                children += j + 1;
                i += j + 1;
            }
            return children;
        }
    }
    /// <summary>
    /// Used to create an error for a node or decorator
    /// </summary>
    public struct Error
    {
        /// <summary>
        /// Severity of the error
        /// </summary>
        public enum Severity
        {
            Info,
            Warning,
            Error
        }
        /// <summary>
        /// Error message
        /// </summary>
        public string message;
        /// <summary>
        /// Severity of the error
        /// </summary>
        public Severity severity;
        public Error(string message, Severity severity)
        {
            this.message = message;
            this.severity = severity;
        }
    }
}