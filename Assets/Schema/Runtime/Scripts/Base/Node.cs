using UnityEngine;
using System.Linq;
using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEditor;
using Schema.Internal;

namespace Schema
{
    /// <summary>
    /// Base class for all Schema nodes.
    /// </summary>

    [Serializable]
    public abstract class Node : GraphObject
    {
        /// <summary>
        /// The parent of this node
        /// </summary>
        public Node parent { get { return m_parent; } private set { m_parent = value; } }
        [SerializeField, HideInInspector] private Node m_parent;
        /// <summary>
        /// An array containing the children of this node
        /// </summary>
        public Node[] children { get { return m_children; } internal set { m_children = value; } }
        [SerializeField, HideInInspector] private Node[] m_children = Array.Empty<Node>();
        /// <summary>
        /// An array containing the conditionals for this node
        /// </summary>
        public Conditional[] conditionals { get { return m_conditionals; } internal set { m_conditionals = value; } }
        [SerializeField, HideInInspector] private Conditional[] m_conditionals = Array.Empty<Conditional>();
        public Modifier[] modifiers { get { return m_modifiers; } internal set { m_modifiers = value; } }
        [SerializeField, HideInInspector] private Modifier[] m_modifiers = Array.Empty<Modifier>();
        /// <summary>
        /// Position of the Node in the graph
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
#else
                parent?.OrderChildren();
#endif
            }
        }
        [SerializeField, HideInInspector] private Vector2 m_graphPosition;
        /// <summary>
        /// Priority for the node
        /// </summary>
        public int priority { get { return m_priority; } internal set { m_priority = value; } }
        [SerializeField, HideInInspector] private int m_priority;
        /// <summary>
        /// Graph for this node
        /// </summary>
        public Graph graph { get { return m_graph; } internal set { m_graph = value; } }
        [SerializeField, HideInInspector] private Graph m_graph;
        /// <summary>
        /// Comment for this node
        /// </summary>
        public string comment { get { return m_comment; } internal set { m_comment = value; } }
        [SerializeField, HideInInspector, TextArea] private string m_comment;
        /// <summary>
        /// Whether to allow the status indicator for this node in the editor
        /// </summary>
        public bool enableStatusIndicator { get { return m_enableStatusIndicator; } private set { m_enableStatusIndicator = value; } }
        [Tooltip("Toggle the status indicator for this node"), HideInInspector, SerializeField] private bool m_enableStatusIndicator = true;
        private string _description;
        private bool didGetDescriptionAttribute;
        /// <summary>
        /// How this node can be connected to other nodes (do not override)
        /// </summary>
        public virtual ConnectionDescriptor connectionDescriptor => ConnectionDescriptor.Both;
        /// <summary>
        /// Description for this node, given by the Description attribute
        /// </summary>
        public string description
        {
            get
            {
                if (!didGetDescriptionAttribute)
                {
                    didGetDescriptionAttribute = true;
                    _description = GetType().GetCustomAttribute<DescriptionAttribute>()?.description;
                }

                return _description;
            }
        }
        internal Type GetMemoryType()
        {
            Type[] types = GetType().GetTypeInfo().DeclaredNestedTypes.ToArray();

            if (types.Length == 0)
                return null;

            return types[0];
        }
        /// <summary>
        /// Whether a parent node is allowed for this node
        /// </summary>
        public virtual bool CanHaveParent() { return parent == null; }
        /// <summary>
        /// Whether children nodes are allowed for this node
        /// </summary>
        public virtual bool CanHaveChildren() { return true; }
        /// <summary>
        ///	Override to allow for Gizmo visualization in the scene view. This will be called only for the currently selected SchemaAgent. 
        /// </summary>
        public virtual void DrawGizmos(SchemaAgent agent) { }
        /// <summary>
        /// Verifies the order of the child list by position
        /// </summary>
        public void VerifyOrder()
        {
            Array.Sort(m_children, (x, y) => x.graphPosition.x < y.graphPosition.x ? -1 : 1);
        }
        /// <summary>
        /// Gets a list of all children attached directly or indirectly to this node (including self)
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
        /// <summary>
        /// Define a custom category for a node
        /// </summary>
        [System.AttributeUsage(AttributeTargets.Class)]
        protected class CategoryAttribute : System.Attribute
        {
            public string category;
            /// <summary>
            /// Define a custom category for a node
            /// </summary>
            /// <param name="category">Content to use for the category in the search menu</param>
            public CategoryAttribute(string category)
            {
                this.category = category;
            }
        }
        public enum ConnectionDescriptor
        {
            OnlyInConnection,
            OnlyOutConnection,
            Floating,
            Both
        }
        public static string GetNodeCategory(Type nodeType)
        {
            CategoryAttribute attribute = nodeType.GetCustomAttribute<CategoryAttribute>();

            return attribute?.category;
        }
        public static string GetNodeDescription(Type nodeType)
        {
            DescriptionAttribute attribute = nodeType.GetCustomAttribute<DescriptionAttribute>();

            return attribute?.description;
        }
        public static Dictionary<string, IEnumerable<Type>> GetNodeCategories()
        {
            Dictionary<string, List<Type>> dict = new Dictionary<string, List<Type>>();

            foreach (Type nodeType in Schema.Utilities.HelperMethods.GetEnumerableOfType(typeof(Node)))
            {
                CategoryAttribute attribute = nodeType.GetCustomAttribute<CategoryAttribute>();

                List<Type> t = null;

                string key = attribute == null ? "" : attribute.category;

                dict.TryGetValue(key, out t);

                if (t == null)
                    dict[key] = new List<Type>() { nodeType };
                else
                    t.Add(nodeType);
            }

            return dict.ToDictionary(x => x.Key, x => x.Value.AsEnumerable());
        }
#if UNITY_EDITOR
        /// <summary>
        /// Order child list by their graph positions
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
        /// Add a connection to another node
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
        /// Whether this node is connected to another
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
            to.children = new Node[] { child };
            child.parent = to;
        }
        /// <summary>
        /// Disconnect from another child node
        /// </summary>
        /// <param name="from">Node to disconnect from. Must be a child of this node</param>
        /// <param name="actionName">Name of the undo action</param>
        /// <param name="undo">Whether to register this operation in the undo stack</param>
        public void RemoveConnection(Node from, string actionName = "Remove Connection", bool undo = true)
        {
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
        /// Remove the connection between this node and its parent
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
        /// Remove connections between this node and its children
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
        /// Breaks connections between this node and its parent and children
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

            parent?.RemoveConnection(this, actionName: "", undo);

            foreach (Node child in children)
                RemoveConnection(child, actionName: "", undo);

            if (undo)
            {
                Undo.SetCurrentGroupName(actionName);
                Undo.CollapseUndoOperations(groupIndex);
            }
        }
        /// <summary>
        /// Breaks connections with parent and children without affecting them
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
        /// Add a conditional to this node
        /// </summary>
        /// <param name="conditionalType">Type of conditional to add. Must inherit from type Conditional</param>
        /// <param name="undo">Whether to register this operation in the undo stack</param>
        /// <returns>Created Conditional</returns>
        /// <exception cref="ArgumentException">conditionalType does not inherit from Conditional</exception>
        public Conditional AddConditional(Type conditionalType, bool undo = true)
        {
            if (!typeof(Conditional).IsAssignableFrom(conditionalType))
                throw new ArgumentException("conditionalType does not inherit from type conditional");

            Conditional conditional = (Conditional)ScriptableObject.CreateInstance(conditionalType);
            conditional.hideFlags = HideFlags.HideInHierarchy;
            conditional.node = this;

            string path = AssetDatabase.GetAssetPath(this);

            if (!String.IsNullOrEmpty(path))
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
        /// Duplicate a given conditional
        /// </summary>
        /// <param name="conditional">Conditional to duplicate</param>
        /// <param name="undo">Whether to register this operation in the undo stack</param>
        /// <returns>Duplciated conditional</returns>
        public Conditional Duplciateconditional(Conditional conditional, bool undo = true)
        {
            Conditional duplicate = ScriptableObject.Instantiate<Conditional>(conditional);
            duplicate.hideFlags = HideFlags.HideAndDontSave;
            conditional.node = this;

            string path = AssetDatabase.GetAssetPath(this);

            if (!String.IsNullOrEmpty(path))
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
        /// Deletes a conditional from this node
        /// </summary>
        /// <param name="conditional">Conditional to remove</param>
        /// <param name="actionName">Name of the undo action</param>
        /// <param name="undo">Whether to register this operation in the undo stack</param>
        public void RemoveConditional(Conditional conditional, string actionName = "Remove conditional", bool undo = true)
        {
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
                ScriptableObject.DestroyImmediate(conditional, true);
            }
        }
        /// <summary>
        /// Add a modifier to the node
        /// </summary>
        /// <param name="modifierType">The type of the modifier to add</param>
        /// <param name="undo">Whether to register this operation in the undo stack</param>
        public Modifier AddModifier(Type modifierType, bool undo = true)
        {
            if (!typeof(Modifier).IsAssignableFrom(modifierType))
                throw new ArgumentException("modifierType does not inherit from type Modifier");

            if (GetType() == typeof(Root))
                return null;

            Modifier modifier = (Modifier)ScriptableObject.CreateInstance(modifierType);
            modifier.hideFlags = HideFlags.HideInHierarchy;
            modifier.node = this;

            string path = AssetDatabase.GetAssetPath(this);

            if (!String.IsNullOrEmpty(path))
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
        /// Remove all null nodes attached to this one
        /// </summary>
        public void PurgeNull()
        {
            Node[] n = m_children;

            foreach (Node node in n)
            {
                if (node == null)
                    ArrayUtility.Remove(ref m_children, node);
            }
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