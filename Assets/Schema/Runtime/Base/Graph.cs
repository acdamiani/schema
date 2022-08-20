using System;
using System.Collections.Generic;
using System.Linq;
using Schema.Internal;
using UnityEditor;
using UnityEngine;

namespace Schema
{
    [CreateAssetMenu(menuName = "Schema/Behavior Tree", order = 84), Serializable]
    public class Graph : ScriptableObject
    {
        [SerializeField] private Root m_root;
        [SerializeField] private Blackboard m_blackboard;
        [SerializeField] private Node[] m_nodes = Array.Empty<Node>();
        [SerializeField] private float m_zoom = 1f;
        [SerializeField] private Vector2 m_pan;

        /// <summary>
        ///     Root of graph
        /// </summary>
        public Root root => m_root;

        /// <summary>
        ///     Blackboard for this entry
        /// </summary>
        public Blackboard blackboard => m_blackboard;

        /// <summary>
        ///     Array of nodes for the graph
        /// </summary>
        public Node[] nodes => m_nodes;

        // public List<Node> nodes = new List<Node>();
        /// <summary>
        ///     Zoom level of the graph view
        /// </summary>
        public float zoom
        {
            get => m_zoom;
            set => m_zoom = value;
        }

        /// <summary>
        ///     Pan vector of the graph view
        /// </summary>
        public Vector2 pan
        {
            get => m_pan;
            set => m_pan = value;
        }
        #if UNITY_EDITOR
        public void Initialize()
        {
            if (blackboard == null)
            {
                m_blackboard = CreateInstance<Blackboard>();
                blackboard.hideFlags = HideFlags.HideInHierarchy;

                string path = AssetDatabase.GetAssetPath(this);

                if (!string.IsNullOrEmpty(path))
                    AssetDatabase.AddObjectToAsset(blackboard, path);
            }

            if (root == null)
                m_root = AddNode<Root>(Vector2.zero);

            PurgeNull();
            Traverse();
        }

        /// <summary>
        ///     Add a node to the tree
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

            Node node = (Node)CreateInstance(nodeType);
            node.hideFlags = HideFlags.HideInHierarchy;
            node.graphPosition = position;
            node.graph = this;

            string path = AssetDatabase.GetAssetPath(this);

            if (!string.IsNullOrEmpty(path))
                AssetDatabase.AddObjectToAsset(node, path);

            if (undo)
            {
                Undo.RegisterCreatedObjectUndo(node, "Node Created");
                Undo.RegisterCompleteObjectUndo(this, "Node Added");
            }

            ArrayUtility.Add(ref m_nodes, node);

            return node;
        }

        /// <summary>
        ///     Add an existing node to the tree
        /// </summary>
        /// <param name="node">Node to add to the tree</param>
        /// <param name="position">Position of the node within the graph</param>
        /// <param name="undo">Wehether to register this operation to undo</param>
        public void AddNode(Node node, Vector2 position, bool undo = true)
        {
            string path = AssetDatabase.GetAssetPath(this);

            node.hideFlags = HideFlags.HideInHierarchy;
            node.graphPosition = position;
            node.graph = this;

            if (!string.IsNullOrEmpty(path))
                AssetDatabase.AddObjectToAsset(node, path);

            if (undo)
                Undo.RegisterCompleteObjectUndo(this, "Node Added");

            ArrayUtility.Add(ref m_nodes, node);
        }

        /// <summary>
        ///     Add an existing node to the tree
        /// </summary>
        /// <param name="node">Node to add to the tree</param>
        /// <param name="undo">Wehether to register this operation to undo</param>
        public void AddNode(Node node, bool undo = true)
        {
            AddNode(node, node.graphPosition, undo);
        }

        /// <summary>
        ///     Duplicates a given node and adds it to the tree.
        /// </summary>
        /// <param name="node">Node to use as the duplicate base</param>
        /// <param name="newPosition">New position of the node within the graph</param>
        /// <param name="undo">Whether to register this operation to undo</param>
        /// <returns>Duplicated node</returns>
        public Node Duplicate(Node node, Vector2 newPosition, bool undo = true)
        {
            Node duplicate = Instantiate(node);

            duplicate.name = node.name;
            duplicate.BreakConnectionsIsolated(undo: false);
            duplicate.ResetGUID();
            duplicate.hideFlags = HideFlags.HideInHierarchy;
            duplicate.graphPosition = newPosition;
            duplicate.graph = this;
            duplicate.children = (Node[])node.children.Clone();

            string path = AssetDatabase.GetAssetPath(this);

            if (!string.IsNullOrEmpty(path))
                AssetDatabase.AddObjectToAsset(duplicate, path);

            if (undo)
            {
                Undo.RegisterCreatedObjectUndo(duplicate, "Node Duplicated");
                Undo.RegisterCompleteObjectUndo(this, "Node Duplicated");
            }

            ArrayUtility.Add(ref m_nodes, duplicate);

            return duplicate;
        }

        /// <summary>
        ///     Duplicate a list of nodes, preserving their connections
        /// </summary>
        /// <param name="nodes">Array or list of nodes to duplicate</param>
        /// <param name="offset">Offset in position that duplicates will have</param>
        /// <param name="undo">Whether to register this operation to undo</param>
        /// <returns>The duplicated list of nodes</returns>
        public IEnumerable<Node> Duplicate(IEnumerable<Node> nodes, Vector2 offset, bool undo = true)
        {
            int groupIndex = -1;

            if (undo)
            {
                Undo.IncrementCurrentGroup();
                groupIndex = Undo.GetCurrentGroup();
            }

            List<Node> original = new List<Node>(nodes);
            List<Node> roots = original.FindAll(node =>
                {
                    if (node.GetType() == typeof(Root))
                        return false;

                    if (node.parent == null)
                        return true;

                    if (!original.Contains(node.parent))
                        return true;

                    return false;
                }
            );

            List<Node> dupl = new List<Node>();

            foreach (Node root in roots)
                dupl.AddRange(DuplicateRecursive(nodes, root, offset, undo).GetAllChildren());

            Traverse();

            if (undo)
            {
                Undo.SetCurrentGroupName("Duplicate");
                Undo.CollapseUndoOperations(groupIndex);
            }

            return dupl;
        }

        private Node DuplicateRecursive(IEnumerable<Node> toDuplicate, Node node, Vector2 offset, bool undo = true)
        {
            Node duplicate = Duplicate(node, node.graphPosition + offset, undo);

            List<Node> duplicateChildren = duplicate.children.ToList();

            duplicateChildren.RemoveAll(n => !toDuplicate.Contains(n));
            duplicateChildren = duplicateChildren.Select(n => DuplicateRecursive(toDuplicate, n, offset, undo))
                .ToList();

            duplicate.BreakConnectionsIsolated(undo: false);

            foreach (Node child in duplicateChildren)
                duplicate.AddConnection(child, undo: false);

            node.conditionals = node.conditionals.Select(x => node.DuplicateConditional(x, undo)).ToArray();

            return duplicate;
        }

        /// <summary>
        ///     Add a node to the tree
        /// </summary>
        /// <typeparam name="T">Type of node to add. Must inherit from Node</typeparam>
        /// <param name="position">Position of the node within the graph</param>
        /// <param name="undo">Whether to register this operation to undo</param>
        /// <returns>The node created in the tree</returns>
        public T AddNode<T>(Vector2 position, bool undo = true) where T : Node
        {
            return (T)AddNode(typeof(T), position);
        }

        public void DeleteNode(Node node)
        {
            if (node.GetType() == typeof(Root) || !ArrayUtility.Contains(m_nodes, node))
                return;

            Undo.RegisterCompleteObjectUndo(this, "Delete Node");
            ArrayUtility.Remove(ref m_nodes, node);

            node.parent?.RemoveConnection(node, "");

            foreach (Node child in node.children)
                node.RemoveConnection(child, "");

            foreach (Conditional conditional in node.conditionals)
                node.RemoveConditional(conditional, "");

            foreach (Modifier modifier in node.modifiers)
                node.RemoveModifier(modifier);

            Undo.DestroyObjectImmediate(node);
        }

        /// <summary>
        ///     Remove multiple nodes from the tree
        /// </summary>
        /// <param name="nodes">List to remove</param>
        public void DeleteNodes(IEnumerable<Node> nodes)
        {
            IEnumerable<Node> nodesWithoutRoot = nodes.Where(node => node.GetType() != typeof(Root))
                .OrderByDescending(node => node.priority);

            Undo.RegisterCompleteObjectUndo(this, "Delete Nodes");
            m_nodes = this.nodes.Except(nodesWithoutRoot).ToArray();

            foreach (Node node in nodesWithoutRoot)
            {
                node.parent?.RemoveConnection(node, "");

                foreach (Node child in node.children)
                    node.RemoveConnection(child, "");

                foreach (Conditional decorator in node.conditionals)
                    node.RemoveConditional(decorator, "");

                Undo.DestroyObjectImmediate(node);
            }
        }

        /// <summary>
        ///     Remove all connections for multiple nodes
        /// </summary>
        /// <param name="nodes">List to break</param>
        public void BreakConnections(IEnumerable<Node> nodes)
        {
            Undo.IncrementCurrentGroup();
            int groupIndex = Undo.GetCurrentGroup();

            foreach (Node node in nodes)
            {
                node.parent?.RemoveConnection(node, "");

                foreach (Node child in node.children)
                    node.RemoveConnection(child, "");
            }

            Traverse();

            Undo.SetCurrentGroupName("Break Connections");
            Undo.CollapseUndoOperations(groupIndex);
        }

        /// <summary>
        ///     Recalculate priorities for all nodes
        /// </summary>
        public void Traverse()
        {
            IEnumerable<Node> nodeList = nodes.Where(node => node != null);

            foreach (Node node in nodeList)
                node.priority = 0;

            TraverseSubtree(root, 1);
        }

        private int TraverseSubtree(Node node, int i)
        {
            node.priority = i;
            int children = 0;
            foreach (Node child in node.children)
            {
                int j = TraverseSubtree(child, i + 1);
                children += j + 1;
                i += j + 1;
            }

            return children;
        }

        /// <summary>
        ///     Removes all null nodes from the tree
        /// </summary>
        public void PurgeNull()
        {
            Node[] n = m_nodes;

            foreach (Node node in n)
                if (node == null)
                    ArrayUtility.Remove(ref m_nodes, node);
                else
                    node.PurgeNull();
        }
        #endif
    }
}