using UnityEngine;
using System.Linq;
using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEditor;

namespace Schema
{
    /// <summary>
    /// Base class for all Schema nodes.
    /// </summary>

    [Serializable]
    public abstract class Node : ScriptableObject
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
        /// An array containing the decorators for this node
        /// </summary>
        public Decorator[] decorators { get { return m_decorators; } internal set { m_decorators = value; } }
        [SerializeField, HideInInspector] private Decorator[] m_decorators = Array.Empty<Decorator>();
        /// <summary>
        /// The GUID for the node
        /// </summary>
        public string uID { get { return m_uID; } }
        [SerializeField, HideInInspector] private string m_uID;
        /// <summary>
        /// Position of the Node in the graph
        /// </summary>
        public Vector2 position { get { return m_position; } set { m_position = value; } }
        [SerializeField, HideInInspector] private Vector2 m_position;
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
            {
                return null;
            }

            return types[0];
        }
        /// <summary>
        /// Determine whether the Node can have more children attached to it
        /// </summary>
        public bool CanHaveChildren()
        {
            return maxChildren > 0 && m_children.Length < maxChildren;
        }
        /// <summary>
        /// Called during OnEnable(). Override this method instead of declaring an OnEnable method to avoid errors
        /// </summary>
        protected virtual void OnNodeEnable() { }
        /// <summary>
        ///	Override to allow for Gizmo visualization in the scene view. This will be called only for the currently selected SchemaAgent. 
        /// </summary>
        public virtual void DrawGizmos(SchemaAgent agent) { }
        /// <summary>
        /// Whether a parent node is allowed for this node:w
        /// </summary>
        public virtual bool canHaveParent { get { return true; } }
        /// <summary>
        /// The maximum allowed number of children for this node
        /// </summary>
        public virtual int maxChildren { get { return Int32.MaxValue; } }
        void OnEnable()
        {
            NameAttribute attribute = GetType().GetCustomAttribute<NameAttribute>();

            if (String.IsNullOrEmpty(name))
                name = attribute != null ? attribute.name : String.Concat(this.GetType().Name.Select(x => Char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' ');

            if (string.IsNullOrEmpty(uID)) m_uID = Guid.NewGuid().ToString("N");

            OnNodeEnable();

            GenerateIcon();
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

            to.parent = this;
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
            from.parent = null;
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
        /// Add a decorator to this node
        /// </summary>
        /// <param name="decoratorType">Type of decorator to add. Must inherit from type Decorator</param>
        /// <param name="undo">Whether to register this operation in the undo stack</param>
        /// <returns>Created decorator</returns>
        /// <exception cref="ArgumentException">decoratorType does not inherit from Decorator</exception>
        public Decorator AddDecorator(Type decoratorType, bool undo = true)
        {
            if (!typeof(Decorator).IsAssignableFrom(decoratorType))
                throw new ArgumentException("decoratorType does not inherit from type Node");

            if (GetType() == typeof(Root))
                return null;

            Decorator decorator = (Decorator)ScriptableObject.CreateInstance(decoratorType);
            decorator.hideFlags = HideFlags.HideInHierarchy;
            decorator.node = this;

            string path = AssetDatabase.GetAssetPath(this);

            if (!String.IsNullOrEmpty(path))
                AssetDatabase.AddObjectToAsset(decorator, path);

            if (undo)
            {
                Undo.RegisterCreatedObjectUndo(decorator, "Decorator Created");
                Undo.RegisterCompleteObjectUndo(this, "Decorator Added");
            }

            ArrayUtility.Add(ref m_decorators, decorator);

            return decorator;
        }
        /// <summary>
        /// Duplicate a given decorator
        /// </summary>
        /// <param name="decorator">Decorator to duplicate</param>
        /// <param name="undo">Whether to register this operation in the undo stack</param>
        /// <returns>Duplciated decorator</returns>
        public Decorator DuplciateDecorator(Decorator decorator, bool undo = true)
        {
            Decorator duplicate = ScriptableObject.Instantiate<Decorator>(decorator);
            duplicate.hideFlags = HideFlags.HideAndDontSave;
            decorator.node = this;

            string path = AssetDatabase.GetAssetPath(this);

            if (!String.IsNullOrEmpty(path))
                AssetDatabase.AddObjectToAsset(duplicate, path);

            if (undo)
            {
                Undo.RegisterCreatedObjectUndo(duplicate, "Decorator Duplicated");
                Undo.RegisterCompleteObjectUndo(this, "Decorator Duplicated");
            }

            ArrayUtility.Add(ref m_decorators, duplicate);

            return duplicate;
        }
        /// <summary>
        /// Verifies the order of the child list by position
        /// </summary>
        public void VerifyOrder()
        {
            Array.Sort(m_children, (x, y) => x.position.x < y.position.x ? -1 : 1);
        }
        /// <summary>
        /// Deletes a decorator from this node
        /// </summary>
        /// <param name="decorator">Decorator to remove</param>
        /// <param name="actionName">Name of the undo action</param>
        /// <param name="undo">Whether to register this operation in the undo stack</param>
        public void RemoveDecorator(Decorator decorator, string actionName = "Remove Decorator", bool undo = true)
        {
            if (!ArrayUtility.Contains(m_decorators, decorator))
            {
                Debug.LogWarning($"Decorator {decorator.name} does not exit on node {name}");
                return;
            }

            if (undo)
            {
                Undo.RegisterCompleteObjectUndo(this, actionName);
                ArrayUtility.Remove(ref m_decorators, decorator);
                Undo.DestroyObjectImmediate(decorator);
            }
            else
            {
                ArrayUtility.Remove(ref m_decorators, decorator);
                ScriptableObject.DestroyImmediate(decorator, true);
            }
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
        internal void ResetGUID()
        {
            m_uID = Guid.NewGuid().ToString("N");
        }
        private void GenerateIcon()
        {
            if (String.IsNullOrEmpty(_darkIconLocation))
            {
                DarkIconAttribute attribute = (DarkIconAttribute)Attribute.GetCustomAttribute(GetType(), typeof(DarkIconAttribute));

                if (attribute == null)
                    _darkIconLocation = "NOT FOUND";
                else
                    _darkIconLocation = attribute.location;
            }
            if (String.IsNullOrEmpty(_lightIconLocation))
            {
                LightIconAttribute attribute = (LightIconAttribute)Attribute.GetCustomAttribute(GetType(), typeof(LightIconAttribute));

                if (attribute == null)
                    _lightIconLocation = "NOT FOUND";
                else
                    _lightIconLocation = attribute.location;
            }

            //Use dark texture
            if (EditorGUIUtility.isProSkin && !_darkIconLocation.Equals("NOT FOUND"))
            {
                Type iconType = _darkIconLocation.StartsWith("c_") ? Schema.Utilities.HelperMethods.FindType("UnityEngine." + _darkIconLocation.Substring(2)) : null;

                Debug.Log(iconType);

                _icon = iconType != null ? (Texture2D)EditorGUIUtility.ObjectContent(null, iconType).image : Resources.Load<Texture2D>(_darkIconLocation);
            }
            else if (!String.IsNullOrEmpty(_lightIconLocation) && !_lightIconLocation.Equals("NOT FOUND"))
            {
                Type iconType = _lightIconLocation.StartsWith("c_") ? Type.GetType(_lightIconLocation.Substring(2)) : null;

                _icon = iconType != null ? (Texture2D)EditorGUIUtility.ObjectContent(null, iconType).image : Resources.Load<Texture2D>(_lightIconLocation);
            }

            usingProTextures = EditorGUIUtility.isProSkin;
        }
        /// <summary>
        /// The current errors for this node
        /// </summary>
        /// <returns>A list of errors to display in the editor</returns>
        public virtual List<Error> GetErrors() { return new List<Error>(); }
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
        public static Texture2D GetNodeIcon(Type type)
        {
            string darkLocation;
            string lightLocation;

            DarkIconAttribute attribute = type.GetCustomAttribute<DarkIconAttribute>();
            LightIconAttribute attribute1 = type.GetCustomAttribute<LightIconAttribute>();

            if (attribute == null)
                darkLocation = "NOT FOUND";
            else
                darkLocation = attribute.location;

            if (attribute == null)
                lightLocation = "NOT FOUND";
            else
                lightLocation = attribute.location;

            //Use dark texture
            if (EditorGUIUtility.isProSkin && !darkLocation.Equals("NOT FOUND"))
            {
                Type iconType = darkLocation.StartsWith("c_") ? Schema.Utilities.HelperMethods.FindType("UnityEngine." + darkLocation.Substring(2)) : null;

                Debug.Log(iconType);

                return iconType != null ? (Texture2D)EditorGUIUtility.ObjectContent(null, iconType).image : Resources.Load<Texture2D>(darkLocation);
            }
            else if (!String.IsNullOrEmpty(lightLocation) && !lightLocation.Equals("NOT FOUND"))
            {
                Type iconType = lightLocation.StartsWith("c_") ? Type.GetType(lightLocation.Substring(2)) : null;

                return iconType != null ? (Texture2D)EditorGUIUtility.ObjectContent(null, iconType).image : Resources.Load<Texture2D>(lightLocation);
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// Where Schema should load the dark mode icon within a resources folder
        /// </summary>
        [System.AttributeUsage(AttributeTargets.Class)]
        protected class DarkIconAttribute : System.Attribute
        {
            public string location;
            /// <summary>
            /// Where Schema should load the dark mode icon within a resources folder
            /// </summary>
            /// <param name="location">Location of the icon to be loaded with Resources.Load</param>
            public DarkIconAttribute(string location)
            {
                this.location = location;
            }
        }
        /// <summary>
        /// Where Schema should load the light mode icon within a resources folder
        /// </summary>
        [System.AttributeUsage(AttributeTargets.Class)]
        protected class LightIconAttribute : System.Attribute
        {
            public string location;
            /// <summary>
            /// Where Schema should load the light mode icon within a resources folder
            /// </summary>
            /// <param name="location">Location of the icon to be loaded with Resources.Load</param>
            public LightIconAttribute(string location)
            {
                this.location = location;
            }
        }
        /// <summary>
        /// Attribute for adding a description to a node in the Editor
        /// </summary>
        [System.AttributeUsage(AttributeTargets.Class)]
        protected class DescriptionAttribute : System.Attribute
        {
            public string description;
            /// <summary>
            /// Attribute for adding a description to a node in the Editor
            /// </summary>
            /// <param name="description">Description for the node</param>
            public DescriptionAttribute(string description)
            {
                this.description = description;
            }
        }
        /// <summary>
        /// Attribute to override the default name of the node in the editor
        /// </summary>
        [System.AttributeUsage(AttributeTargets.Class)]
        protected class NameAttribute : System.Attribute
        {
            public string name;
            /// <summary>
            /// Attribute to override the default name of the node in the edit or
            /// </summary>
            /// <param name="name">Default name of the node to use</param>
            public NameAttribute(string name)
            {
                this.name = name;
            }
        }
#if UNITY_EDITOR
        private Texture2D _icon;
        private string _darkIconLocation;
        private string _lightIconLocation;
        /// <summary>
        /// Icon of the node (editor only)
        /// </summary>
        public Texture2D icon
        {
            get
            {
                if (usingProTextures != EditorGUIUtility.isProSkin)
                    GenerateIcon();

                return _icon;
            }
        }
        private bool usingProTextures;
#endif
    }
}