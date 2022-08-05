using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Schema;

namespace Schema.Internal
{
    public abstract class GraphObject : ScriptableObject
    {
        /// <summary>
        /// The GUID for this object
        /// </summary>
        public string uID { get { return m_uID; } }
        [SerializeField, HideInInspector] private string m_uID = Guid.NewGuid().ToString("N");
#if UNITY_EDITOR
        /// <summary>
        /// The icon for this object
        /// </summary>
        public Texture2D icon { get { return m_icon; } }
        private Texture2D m_icon;
#endif
        /// <summary>
        /// Use this instead of OnEnable 
        /// </summary>
        protected virtual void OnObjectEnable() { }
        void OnEnable()
        {
            NameAttribute attribute = GetType().GetCustomAttribute<NameAttribute>();

            if (String.IsNullOrEmpty(name))
                name = attribute != null ? attribute.name : String.Concat(this.GetType().Name.Select(x => Char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' ');


#if UNITY_EDITOR
            m_icon = GetIcon(GetType());
#endif

            OnObjectEnable();
        }
#if UNITY_EDITOR
        /// <summary>
        /// Get the icon for a specified type
        /// </summary>
        /// <typeparam name="T">The type of the icon</typeparam>
        /// <returns>The texture for the specified type</returns>
        public static Texture2D GetIcon<T>() where T : GraphObject
        {
            return GetIcon(typeof(T));
        }
        /// <summary>
        /// Get the icon for a specified type
        /// </summary>
        /// <param name="type">The type of the icon</param>
        /// <returns>The texture for the specified type</returns>
        public static Texture2D GetIcon(Type type)
        {
            if (!(typeof(GraphObject).IsAssignableFrom(type)))
                throw new ArgumentException("Type parameter does not inherit from GraphObject");

            DarkIconAttribute darkIcon = type.GetCustomAttribute<DarkIconAttribute>();
            LightIconAttribute lightIcon = type.GetCustomAttribute<LightIconAttribute>();

            //Use dark texture
            if (UnityEditor.EditorGUIUtility.isProSkin && darkIcon != null)
            {
                Texture2D ret = darkIcon.isEditorIcon
                    ? (Texture2D)UnityEditor.EditorGUIUtility.IconContent(darkIcon.location).image
                    : Resources.Load<Texture2D>(darkIcon.location);

                return ret;
            }
            else if (lightIcon != null)
            {
                Texture2D ret = lightIcon.isEditorIcon
                    ? (Texture2D)UnityEditor.EditorGUIUtility.IconContent(lightIcon.location).image
                    : Resources.Load<Texture2D>(lightIcon.location);

                return ret;
            }
            else
            {
                return null;
            }
        }
        public void ResetGUID()
        {
            m_uID = Guid.NewGuid().ToString("N");
        }
#endif
        public static string GetDescription<T>() where T : GraphObject
        {
            return GetDescription(typeof(T));
        }
        public static string GetDescription(Type type)
        {
            if (!(typeof(GraphObject).IsAssignableFrom(type)))
                throw new ArgumentException("Type parameter does not inherit from GraphObject");

            DescriptionAttribute description = type.GetCustomAttribute<DescriptionAttribute>();

            return description?.description ?? "";
        }
        public static string GetCategory<T>() where T : GraphObject
        {
            return GetCategory(typeof(T));
        }
        public static string GetCategory(Type type)
        {
            if (!(typeof(GraphObject).IsAssignableFrom(type)))
                throw new ArgumentException("Type parameter does not inherit from GraphObject.");

            CategoryAttribute category = type.GetCustomAttribute<CategoryAttribute>();

            return category?.category ?? "";
        }
        public IEnumerable<Error> GetErrors() { return Enumerable.Empty<Error>(); }
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
        /// <summary>
        /// Where Schema should load the dark mode icon within a resources folder
        /// </summary>
        [System.AttributeUsage(AttributeTargets.Class)]
        protected class DarkIconAttribute : System.Attribute
        {
            public string location;
            public bool isEditorIcon;
            /// <summary>
            /// Where Schema should load the dark mode icon within a resources folder
            /// </summary>
            /// <param name="location">Location of the icon to be loaded with Resources.Load</param>
            public DarkIconAttribute(string location)
            {
                this.location = location;
            }
            /// <summary>
            /// Where Schema should load the dark mode icon within a resources folder
            /// </summary>
            /// <param name="location">Location of the icon to be loaded with Resources.Load, or if isEditorIcon is true, the name of the editor icon to load</param>
            /// <param name="isEditorIcon">Whether the location specified is the name of an editor icon</param>
            public DarkIconAttribute(string location, bool isEditorIcon)
            {
                this.location = location;
                this.isEditorIcon = isEditorIcon;
            }
        }
        /// <summary>
        /// Where Schema should load the light mode icon within a resources folder
        /// </summary>
        [System.AttributeUsage(AttributeTargets.Class)]
        protected class LightIconAttribute : System.Attribute
        {
            public string location;
            public bool isEditorIcon;
            /// <summary>
            /// Where Schema should load the light mode icon within a resources folder
            /// </summary>
            /// <param name="location">Location of the icon to be loaded with Resources.Load</param>
            public LightIconAttribute(string location)
            {
                this.location = location;
            }
            /// <summary>
            /// Where Schema should load the light mode icon within a resources folder
            /// </summary>
            /// <param name="location">Location of the icon to be loaded with Resources.Load, or if isEditorIcon is true, the name of the editor icon to load</param>
            /// <param name="isEditorIcon">Whether the location specified is the name of an editor icon</param>
            public LightIconAttribute(string location, bool isEditorIcon)
            {
                this.location = location;
                this.isEditorIcon = isEditorIcon;
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
    }
}