using System;
using System.Collections.Generic;
using System.Linq;
using Schema.Internal;
using UnityEditor;
using UnityEngine;

namespace Schema
{
    /// <summary>
    ///     Base class for all Schema nodes.
    /// </summary>
    [Serializable]
    public abstract class Node : GraphObject
    {
        public enum ConnectionDescriptor
        {
            OnlyInConnection,
            OnlyOutConnection,
            Floating,
            Both
        }

        [SerializeField, HideInInspector] private Node m_parent;
        [SerializeField, HideInInspector] private Node[] m_children = Array.Empty<Node>();
        [SerializeField, HideInInspector] private Conditional[] m_conditionals = Array.Empty<Conditional>();
        [SerializeField, HideInInspector] private Modifier[] m_modifiers = Array.Empty<Modifier>();
        [SerializeField, HideInInspector] private Vector2 m_graphPosition;
        [SerializeField, HideInInspector] private int m_priority;
        [SerializeField, HideInInspector] private Graph m_graph;

        [SerializeField, HideInInspector, TextArea]
        private string m_comment;

        [Tooltip("Toggle the status indicator for this node"), HideInInspector, SerializeField]
        private bool m_enableStatusIndicator = true;

        internal Stack<Modifier.Message> messageStack = new Stack<Modifier.Message>();

        /// <summary>
        ///     The parent of this node
        /// </summary>
        public Node parent
        {
            get => m_parent;
            private set => m_parent = value;
        }

        /// <summary>
        ///     An array containing the children of this node
        /// </summary>
        public Node[] children
        {
            get => m_children;
            internal set => m_children = value;
        }

        /// <summary>
        ///     An array containing the conditionals for this node
        /// </summary>
        public Conditional[] conditionals
        {
            get => m_conditionals;
            internal set => m_conditionals = value;
        }

        public Modifier[] modifiers
        {
            get => m_modifiers;
            internal set => m_modifiers = value;
        }

        /// <summary>
        ///     Position of the Node in the graph
        /// </summary>
        public Vector2 graphPosition
        {
            get { return m_graphPosition; }
            set
            {
                m_graphPosition = value;

                #if UNITY_EDITOR
                if (posNoCheck)
                    modifiedPositions.Add(this);
                else
                    parent?.OrderChildren();
                #endif
            }
        }

        /// <summary>
        ///     Priority for the node
        /// </summary>
        public int priority
        {
            get => m_priority;
            internal set => m_priority = value;
        }

        /// <summary>
        ///     Graph for this node
        /// </summary>
        public Graph graph
        {
            get => m_graph;
            internal set => m_graph = value;
        }

        /// <summary>
        ///     Comment for this node
        /// </summary>
        public string comment
        {
            get => m_comment;
            internal set => m_comment = value;
        }

        /// <summary>
        ///     Whether to allow the status indicator for this node in the editor
        /// </summary>
        public bool enableStatusIndicator
        {
            get => m_enableStatusIndicator;
            private set => m_enableStatusIndicator = value;
        }

        /// <summary>
        ///     How this node can be connected to other nodes (do not override)
        /// </summary>
        public virtual ConnectionDescriptor connectionDescriptor => ConnectionDescriptor.Both;

        /// <summary>
        ///     Get whether this node is in the sub tree of another node
        /// </summary>
        /// <param name="node">Node to check relation to</param>
        public bool IsSubTreeOf(Node node)
        {
            if (node == null)
                return false;

            Node current = this;

            do
            {
                if (node == current)
                    return true;

                current = current.parent;
            } while (current != null);

            return false;
        }

        /// <summary>
        ///     Get whether this node is lower priority of another node (to the right)
        /// </summary>
        /// <param name="node">Node to check relation to</param>
        public bool IsLowerPriority(Node node)
        {
            if (node == null)
                return false;

            return priority > node.priority && !IsSubTreeOf(node);
        }

        /// <summary>
        ///     Whether a parent node is allowed for this node
        /// </summary>
        public virtual bool CanHaveParent()
        {
            return parent == null;
        }

        /// <summary>
        ///     Override this method to draw gizmos for this node in the Scene view
        /// </summary>
        public virtual void DoNodeGizmos(SchemaAgent agent)
        {
        }

        /// <summary>
        ///     Whether children nodes are allowed for this node
        /// </summary>
        public virtual bool CanHaveChildren()
        {
            return true;
        }

        /// <summary>
        ///     Verifies the order of the child list by position
        /// </summary>
        public void VerifyOrder()
        {
            Array.Sort(m_children, (x, y) => x.graphPosition.x < y.graphPosition.x ? -1 : 1);
        }

        /// <summary>
        ///     Gets a list of all children attached directly or indirectly to this node (including self)
        /// </summary>
        /// <returns>List of all children in subtree</returns>
        public IEnumerable<Node> GetAllChildren()
        {
            List<Node> ret = new List<Node>();

            foreach (Node child in children)
                ret.AddRange(child.GetAllChildren());

            ret.Add(this);

            return ret;
        }
#if UNITY_EDITOR
        public static Node Instantiate(Node node)
        {
            Node copy = ScriptableObject.Instantiate(node);

            copy.name = node.name;
            copy.graphPosition += Vector2.one * 32f;
            copy.ResetGUID();
            copy.BreakConnections();
            copy.priority = 0;

            for (int i = 0; i < copy.conditionals.Length; i++)
            {
                Conditional copiedConditional = Conditional.Instantiate(copy.conditionals[i]);

                copiedConditional.node = copy;
                copy.conditionals[i] = copiedConditional;
            }

            for (int i = 0; i < copy.modifiers.Length; i++)
            {
                Modifier copiedModifier = Modifier.Instantiate(copy.modifiers[i]);

                copiedModifier.node = copy;
                copy.modifiers[i] = copiedModifier;
            }

            return copy;
        }

        public static Node Instantiate(Node node, IEnumerable<Conditional> conditionalsToDuplicate)
        {
            Node copy = ScriptableObject.Instantiate(node);

            copy.name = node.name;
            copy.graphPosition += Vector2.one * 32f;
            copy.ResetGUID();
            copy.BreakConnections();
            copy.priority = 0;

            copy.conditionals = conditionalsToDuplicate
                .Intersect(node.conditionals)
                .Select(x =>
                {
                    Conditional duplicatedConditional = Conditional.Instantiate(x);
                    duplicatedConditional.node = copy;

                    return duplicatedConditional;
                })
                .ToArray();

            copy.modifiers = node.modifiers
                .Select(x =>
                {
                    Modifier duplicatedModifier = Modifier.Instantiate(x);
                    duplicatedModifier.node = copy;

                    return duplicatedModifier;
                })
                .ToArray();

            return copy;
        }

        /// <summary>
        ///     Order child list by their graph positions
        /// </summary>
        public void OrderChildren()
        {
            IEnumerable<Node> prev = (IEnumerable<Node>)m_children.Clone();

            m_children = m_children.OrderBy(x => x.graphPosition.x)
                .ToArray();

            if (!prev.SequenceEqual(m_children))
                graph.Traverse();
        }

        /// <summary>
        ///     Add a connection to another node
        /// </summary>
        /// <param name="to">Node to connect to</param>
        /// <param name="actionName">Name of the undo action</param>
        /// <param name="undo">Whether to register this operation in the undo stack</param>
        public void AddConnection(Node to, string actionName = "Add Connection", bool undo = true)
        {
            if (undo)
            {
                Undo.RegisterCompleteObjectUndo(this, actionName);
                Undo.RegisterCompleteObjectUndo(to, actionName);
            }

            if (!m_children.Contains(to))
                ArrayUtility.Add(ref m_children, to);

            to.parent?.RemoveConnection(to);
            to.parent = this;

            OrderChildren();

            graph.Traverse();
        }

        /// <summary>
        ///     Whether this node is connected to another
        /// </summary>
        /// <param name="node">The node to check</param>
        public bool IsConnected(Node node)
        {
            if (node == this)
                return false;

            return node == parent || ArrayUtility.Contains(m_children, node);
        }

        public void SplitConnection(Node to, Node child, string actionName = "Split Connection", bool undo = true)
        {
            if (undo)
            {
                Undo.RegisterCompleteObjectUndo(this, actionName);
                Undo.RegisterCompleteObjectUndo(child, actionName);
                Undo.RegisterCompleteObjectUndo(to, actionName);
            }

            if (!m_children.Contains(child))
                return;

            int i = Array.IndexOf(m_children, child);
            m_children[i] = to;

            to.parent = this;
            to.children = new[] { child };
            child.parent = to;
        }

        /// <summary>
        ///     Disconnect from another child node
        /// </summary>
        /// <param name="from">Node to disconnect from. Must be a child of this node</param>
        /// <param name="actionName">Name of the undo action</param>
        /// <param name="undo">Whether to register this operation in the undo stack</param>
        public void RemoveConnection(Node from, string actionName = "Remove Connection", bool undo = true)
        {
            if (from == null)
                return;

            if (undo)
            {
                Undo.RegisterCompleteObjectUndo(this, actionName);
                Undo.RegisterCompleteObjectUndo(from, actionName);
            }

            if (m_children.Contains(from))
                ArrayUtility.Remove(ref m_children, from);

            if (from.parent == this)
                from.parent = null;

            graph.Traverse();
        }

        /// <summary>
        ///     Remove the connection between this node and its parent
        /// </summary>
        /// <param name="actionName">Name of the undo action</param>
        /// <param name="undo">Whether to register this operation in the undo stack</param>
        public void RemoveParent(string actionName = "Remove Parent Connection", bool undo = true)
        {
            if (parent == null)
                return;

            if (undo)
            {
                Undo.RegisterCompleteObjectUndo(this, actionName);
                Undo.RegisterCompleteObjectUndo(parent, actionName);
            }

            parent.RemoveConnection(this, actionName, undo);
        }

        /// <summary>
        ///     Remove connections between this node and its children
        /// </summary>
        /// <param name="actionName">Name of the undo action</param>
        /// <param name="undo">Whether to register this operation in the undo stack</param>
        public void RemoveChildren(string actionName = "Remove Child Connections", bool undo = true)
        {
            if (undo)
            {
                Undo.RegisterCompleteObjectUndo(this, actionName);

                foreach (Node child in children)
                    Undo.RegisterCompleteObjectUndo(child, actionName);
            }

            foreach (Node child in children)
                RemoveConnection(child, actionName, undo);
        }

        /// <summary>
        ///     Breaks connections between this node and its parent and children
        /// </summary>
        /// <param name="actionName">Name of the undo action</param>
        /// <param name="undo">Whether to register this operation in the undo stack</param>
        public void BreakConnections(string actionName = "Break Connections", bool undo = true)
        {
            int groupIndex = -1;

            if (undo)
            {
                Undo.IncrementCurrentGroup();
                groupIndex = Undo.GetCurrentGroup();
            }

            parent?.RemoveConnection(this, "", undo);

            foreach (Node child in children)
                RemoveConnection(child, "", undo);

            if (undo)
            {
                Undo.SetCurrentGroupName(actionName);
                Undo.CollapseUndoOperations(groupIndex);
            }
        }

        /// <summary>
        ///     Breaks connections with parent and children without affecting them
        /// </summary>
        /// <param name="actionName">Name of the undo action</param>
        /// <param name="undo">Whether to register this operation in the udno stack</param>
        public void BreakConnectionsIsolated(string actionName = "Break Connections", bool undo = true)
        {
            int groupIndex = -1;

            if (undo)
            {
                Undo.IncrementCurrentGroup();
                groupIndex = Undo.GetCurrentGroup();
                Undo.RegisterCompleteObjectUndo(this, actionName);
            }

            parent = null;
            ArrayUtility.Clear(ref m_children);

            if (undo)
            {
                Undo.SetCurrentGroupName(actionName);
                Undo.CollapseUndoOperations(groupIndex);
            }
        }

        /// <summary>
        ///     Add a conditional to this node
        /// </summary>
        /// <param name="conditionalType">Type of conditional to add. Must inherit from type Conditional</param>
        /// <param name="undo">Whether to register this operation in the undo stack</param>
        /// <returns>Created Conditional</returns>
        /// <exception cref="ArgumentException">conditionalType does not inherit from Conditional</exception>
        public Conditional AddConditional(Type conditionalType, bool undo = true)
        {
            if (!typeof(Conditional).IsAssignableFrom(conditionalType))
                throw new ArgumentException("conditionalType does not inherit from type conditional");

            Conditional conditional = (Conditional)CreateInstance(conditionalType);
            conditional.hideFlags = HideFlags.HideInHierarchy;
            conditional.node = this;

            string path = AssetDatabase.GetAssetPath(this);

            if (!string.IsNullOrEmpty(path))
                AssetDatabase.AddObjectToAsset(conditional, path);

            if (undo)
            {
                Undo.RegisterCreatedObjectUndo(conditional, "Conditional Created");
                Undo.RegisterCompleteObjectUndo(this, "Conditional Added");
            }

            ArrayUtility.Add(ref m_conditionals, conditional);

            return conditional;
        }

        /// <summary>
        ///     Add a conditional to this node
        /// </summary>
        /// <param name="conditionalType">Type of conditional to add. Must inherit from type Conditional</param>
        /// <param name="undo">Whether to register this operation in the undo stack</param>
        /// <returns>Created Conditional</returns>
        /// <exception cref="ArgumentException">conditionalType does not inherit from Conditional</exception>
        public void AddConditional(Conditional conditional, bool undo = true)
        {
            if (ArrayUtility.Contains(m_conditionals, conditional))
                return;

            conditional.hideFlags = HideFlags.HideInHierarchy;
            conditional.node = this;

            string path = AssetDatabase.GetAssetPath(this);

            if (!string.IsNullOrEmpty(path))
                AssetDatabase.AddObjectToAsset(conditional, path);

            if (undo)
                Undo.RegisterCompleteObjectUndo(this, "Conditional Added");

            ArrayUtility.Add(ref m_conditionals, conditional);
        }

        /// <summary>
        ///     Duplicate a given conditional
        /// </summary>
        /// <param name="conditional">Conditional to duplicate</param>
        /// <param name="undo">Whether to register this operation in the undo stack</param>
        /// <returns>Duplciated conditional</returns>
        public Conditional DuplicateConditional(Conditional conditional, bool undo = true)
        {
            Conditional duplicate = Instantiate(conditional);
            duplicate.hideFlags = HideFlags.HideAndDontSave;
            conditional.node = this;

            string path = AssetDatabase.GetAssetPath(this);

            if (!string.IsNullOrEmpty(path))
                AssetDatabase.AddObjectToAsset(duplicate, path);

            if (undo)
            {
                Undo.RegisterCreatedObjectUndo(duplicate, "conditional Duplicated");
                Undo.RegisterCompleteObjectUndo(this, "conditional Duplicated");
            }

            ArrayUtility.Add(ref m_conditionals, duplicate);

            return duplicate;
        }

        /// <summary>
        ///     Move a conditional to an index
        /// </summary>
        /// <param name="conditional">The conditional to move</param>
        /// <param name="index">Index to move the conditional to</param>
        /// <param name="undo">Whether to register this operation in the unod stack</param>
        public void MoveConditional(Conditional conditional, int index, bool undo = true)
        {
            if (!conditionals.Contains(conditional))
                return;

            if (undo)
                Undo.RegisterCompleteObjectUndo(this, "Moved Conditional");

            ArrayUtility.Remove(ref m_conditionals, conditional);
            ArrayUtility.Insert(ref m_conditionals, index, conditional);
        }

        /// <summary>
        ///     Deletes a conditional from this node
        /// </summary>
        /// <param name="conditional">Conditional to remove</param>
        /// <param name="actionName">Name of the undo action</param>
        /// <param name="undo">Whether to register this operation in the undo stack</param>
        public void RemoveConditional(Conditional conditional, string actionName = "Remove conditional",
            bool undo = true)
        {
            if (conditional == null)
                return;

            if (!ArrayUtility.Contains(m_conditionals, conditional))
            {
                Debug.LogWarning($"conditional {conditional.name} does not exit on node {name}");
                return;
            }

            if (undo)
            {
                Undo.RegisterCompleteObjectUndo(this, actionName);
                ArrayUtility.Remove(ref m_conditionals, conditional);
                Undo.DestroyObjectImmediate(conditional);
            }
            else
            {
                ArrayUtility.Remove(ref m_conditionals, conditional);
                DestroyImmediate(conditional, true);
            }
        }

        /// <summary>
        ///     Add a modifier to the node
        /// </summary>
        /// <param name="modifierType">The type of the modifier to add</param>
        /// <param name="undo">Whether to register this operation in the undo stack</param>
        public Modifier AddModifier(Type modifierType, bool undo = true)
        {
            if (!typeof(Modifier).IsAssignableFrom(modifierType))
                throw new ArgumentException("modifierType does not inherit from type Modifier");

            if (GetType() == typeof(Root))
                return null;

            Modifier modifier = (Modifier)CreateInstance(modifierType);
            modifier.hideFlags = HideFlags.HideInHierarchy;
            modifier.node = this;

            string path = AssetDatabase.GetAssetPath(this);

            if (!string.IsNullOrEmpty(path))
                AssetDatabase.AddObjectToAsset(modifier, path);

            if (undo)
            {
                Undo.RegisterCreatedObjectUndo(modifier, "Modifier Created");
                Undo.RegisterCompleteObjectUndo(this, "Modifier Added");
            }

            ArrayUtility.Add(ref m_modifiers, modifier);

            return modifier;
        }

        /// <summary>
        ///     Deletes a modifier from this node
        /// </summary>
        /// <param name="modifier">Modifier to remove</param>
        /// <param name="undo">Whether to register this operation in the undo stack</param>
        public void RemoveModifier(Modifier modifier, bool undo = true)
        {
            if (!m_modifiers.Contains(modifier))
            {
                Debug.LogWarning($"conditional {modifier.name} does not exit on node {name}");
                return;
            }

            if (undo)
            {
                Undo.RegisterCompleteObjectUndo(this, "Remove Modifier");
                ArrayUtility.Remove(ref m_modifiers, modifier);
                Undo.DestroyObjectImmediate(modifier);
            }
            else
            {
                ArrayUtility.Remove(ref m_modifiers, modifier);
                DestroyImmediate(modifier, true);
            }
        }

        /// <summary>
        ///     Move a modifier to an index
        /// </summary>
        /// <param name="conditional">The conditional to move</param>
        /// <param name="index">Index to move the conditional to</param>
        /// <param name="undo">Whether to register this operation in the unod stack</param>
        public void MoveModifier(Modifier modifier, int index, bool undo = true)
        {
            if (!m_modifiers.Contains(modifier))
                return;

            if (undo)
                Undo.RegisterCompleteObjectUndo(this, "Move Modifier");

            ArrayUtility.Remove(ref m_modifiers, modifier);
            ArrayUtility.Insert(ref m_modifiers, index, modifier);
        }

        /// <summary>
        ///     Remove all null nodes attached to this one
        /// </summary>
        public void PurgeNull()
        {
            Node[] n = m_children;

            foreach (Node node in n)
                if (node == null)
                    ArrayUtility.Remove(ref m_children, node);
        }

        private static List<Node> modifiedPositions = new List<Node>();
        private static bool posNoCheck;

        public static void BeginPosNoCheck()
        {
            if (posNoCheck)
                Debug.LogWarning("Call EndPosNoCheck before BeginPosNoCheck.");

            posNoCheck = true;
        }

        public static void EndPosNoCheck()
        {
            if (!posNoCheck)
                Debug.LogWarning("Call BeginPosNoCheck before EndPosNoCheck.");

            posNoCheck = false;

            foreach (Node n in modifiedPositions)
                n.parent?.OrderChildren();

            modifiedPositions.Clear();
        }
#endif
    }
}