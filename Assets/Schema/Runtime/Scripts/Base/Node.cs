using UnityEngine;
using System.Linq;
using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEditor;

namespace Schema.Runtime
{
    [Serializable]
    public abstract class Node : ScriptableObject
    {
        [HideInInspector] public Node parent;
        [HideInInspector] public List<Node> children;
        [HideInInspector] public List<Decorator> decorators;
        [HideInInspector] public string uID;
        [HideInInspector] public Vector2 position;
        [HideInInspector] public int priority;
        [HideInInspector] public Graph graph;
        [HideInInspector, TextArea] public string comment;
        [Tooltip("Toggle the status indicator for this node"), HideInInspector] public bool enableStatusIndicator = true;
        [SerializeField, HideInInspector, DisableOnPlay] private string _name;
        private string _description;
        private bool didGetDescriptionAttribute;
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
        public Type GetMemoryType()
        {
            Type[] types = GetType().GetTypeInfo().DeclaredNestedTypes.ToArray();

            if (types.Length == 0)
            {
                return null;
            }

            return types[0];
        }
        /// <summary>
        ///	Override to allow for Gizmo visualization in the scene view. This will be called only for the currently selected SchemaAgent. 
        /// </summary>
        public virtual void DrawGizmos(SchemaAgent agent) { }
        public string Name
        {
            get
            {
                if (String.IsNullOrEmpty(_name))
                    _name = String.Concat(this.GetType().Name.Select(x => Char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' ');

                return _name;
            }
            set
            {
                name = value;
                _name = value;
            }
        }
        public bool canHaveParent
        {
            get
            {
                return (parent == null) && _canHaveParent;
            }
        }
        public bool canHaveChildren
        {
            get
            {
                return children.Count + 1 <= _maxChildren;
            }
        }
        public bool drawInConnection { get { return _canHaveParent; } }
        public bool drawOutConnection { get { return _maxChildren > 0; } }

        public virtual bool _canHaveParent { get { return true; } }
        public virtual int _maxChildren { get { return Int32.MaxValue; } }
        public Node()
        {
            if (children == null)
            {
                children = new List<Node>();
            }

            if (decorators == null)
            {
                decorators = new List<Decorator>();
            }

            if (string.IsNullOrEmpty(uID)) uID = Guid.NewGuid().ToString("N");
        }

        public enum NodeType
        {
            Root,
            Composite,
            Task
        }
        public virtual List<Error> GetErrors() { return new List<Error>(); }

        [System.AttributeUsage(System.AttributeTargets.Class)]
        protected class CategoryAttribute : System.Attribute
        {
            public string category;
            public CategoryAttribute(string category)
            {
                this.category = category;
            }
        }
        private bool? hasCategory = null;
        private CategoryAttribute categoryAttribute;
        public string category
        {
            get
            {
                if (hasCategory == null)
                {
                    Attribute a = Attribute.GetCustomAttribute(GetType(), typeof(CategoryAttribute));

                    if (a != null)
                    {
                        categoryAttribute = (CategoryAttribute)a;
                        hasCategory = true;

                        return categoryAttribute.category;
                    }
                    else
                    {
                        hasCategory = false;
                        return "";
                    }
                }
                else
                {
                    if (hasCategory == true)
                    {
                        return categoryAttribute.category;
                    }
                    else
                    {
                        return "";
                    }
                }
            }
        }

#if UNITY_EDITOR
        private Texture2D _icon;
        private string _darkIconLocation;
        private string _lightIconLocation;
        public Texture2D icon
        {
            get
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

                //if icon is null or the skin has changed
                if (_icon == null || usingProTextures != EditorGUIUtility.isProSkin)
                {
                    //Use dark texture
                    if (EditorGUIUtility.isProSkin && !String.IsNullOrEmpty(_darkIconLocation) && !_darkIconLocation.Equals("NOT FOUND"))
                    {
                        _icon = Resources.Load<Texture2D>(_darkIconLocation);
                    }
                    else if (!String.IsNullOrEmpty(_lightIconLocation) && !_lightIconLocation.Equals("NOT FOUND"))
                    {
                        _icon = Resources.Load<Texture2D>(_lightIconLocation);
                    }

                    usingProTextures = EditorGUIUtility.isProSkin;
                }

                return _icon;
            }
        }
        private bool usingProTextures;

        //initializes node as a "working copy"
        public void WorkingNode()
        {
            foreach (Decorator d in decorators)
            {
                d.hideFlags = HideFlags.HideAndDontSave;
            }

            hideFlags = HideFlags.HideAndDontSave;
        }
        [NonSerialized] public bool dirty;
#endif

        [System.AttributeUsage(AttributeTargets.Class)]
        protected class DarkIconAttribute : System.Attribute
        {
            public string location;
            public DarkIconAttribute(string location)
            {
                this.location = location;
            }
        }
        [System.AttributeUsage(AttributeTargets.Class)]
        protected class LightIconAttribute : System.Attribute
        {
            public string location;
            public LightIconAttribute(string location)
            {
                this.location = location;
            }
        }
        [System.AttributeUsage(AttributeTargets.Class)]
        protected class DescriptionAttribute : System.Attribute
        {
            public string description;
            public DescriptionAttribute(string description)
            {
                this.description = description;
            }
        }
    }
}