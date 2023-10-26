using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Schema.Internal
{
    public abstract class GraphObject : ScriptableObject
    {
        [SerializeField, HideInInspector] private string m_uID = Guid.NewGuid().ToString("N");

        /// <summary>
        ///     The GUID for this object
        /// </summary>
        public string uID => m_uID;

        /// <summary>
        ///     The description for this object
        /// </summary>
        public string description { get; private set; }

        /// <summary>
        ///     The category wof this object
        /// </summary>
        public string category { get; private set; }
#if UNITY_EDITOR
        /// <summary>
        ///     The icon for this object
        /// </summary>
        public Texture2D icon { get; private set; }

#endif

        private void OnEnable()
        {
            Type t = GetType();

            NameAttribute attribute = t.GetCustomAttribute<NameAttribute>();

            if (string.IsNullOrEmpty(name))
                name = attribute != null
                    ? attribute.name
                    : string.Concat(t.Name.Select(x => char.IsUpper(x) ? " " + x : x.ToString()))
                        .TrimStart(' ');

            description = GetDescription(t);
            category = GetCategory(t);

#if UNITY_EDITOR
            icon = GetIcon(t);
#endif

            OnObjectEnable();
        }

        /// <summary>
        ///     Use this instead of OnEnable
        /// </summary>
        protected virtual void OnObjectEnable()
        {
        }

        public static string GetDescription<T>() where T : GraphObject
        {
            return GetDescription(typeof(T));
        }

        public static string GetDescription(Type type)
        {
            if (!typeof(GraphObject).IsAssignableFrom(type))
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
            if (!typeof(GraphObject).IsAssignableFrom(type))
                throw new ArgumentException("Type parameter does not inherit from GraphObject.");

            CategoryAttribute category = type.GetCustomAttribute<CategoryAttribute>();

            return category?.category ?? "";
        }

        public IEnumerable<Error> GetErrors()
        {
            return Enumerable.Empty<Error>();
        }

        /// <summary>
        ///     Attribute to override the default name of the node in the editor
        /// </summary>
        [AttributeUsage(AttributeTargets.Class)]
        protected class NameAttribute : Attribute
        {
            public string name;

            /// <summary>
            ///     Attribute to override the default name of the node in the edit or
            /// </summary>
            /// <param name="name">Default name of the node to use</param>
            public NameAttribute(string name)
            {
                this.name = name;
            }
        }

        /// <summary>
        ///     Where Schema should load the dark mode icon within a resources folder
        /// </summary>
        [AttributeUsage(AttributeTargets.Class)]
        protected class DarkIconAttribute : Attribute
        {
            public bool isEditorIcon;
            public string location;

            /// <summary>
            ///     Where Schema should load the dark mode icon within a resources folder
            /// </summary>
            /// <param name="location">Location of the icon to be loaded with Resources.Load</param>
            public DarkIconAttribute(string location)
            {
                this.location = location;
            }

            /// <summary>
            ///     Where Schema should load the dark mode icon within a resources folder
            /// </summary>
            /// <param name="location">
            ///     Location of the icon to be loaded with Resources.Load, or if isEditorIcon is true, the name of
            ///     the editor icon to load
            /// </param>
            /// <param name="isEditorIcon">Whether the location specified is the name of an editor icon</param>
            public DarkIconAttribute(string location, bool isEditorIcon)
            {
                this.location = location;
                this.isEditorIcon = isEditorIcon;
            }
        }

        /// <summary>
        ///     Where Schema should load the light mode icon within a resources folder
        /// </summary>
        [AttributeUsage(AttributeTargets.Class)]
        protected class LightIconAttribute : Attribute
        {
            public bool isEditorIcon;
            public string location;

            /// <summary>
            ///     Where Schema should load the light mode icon within a resources folder
            /// </summary>
            /// <param name="location">Location of the icon to be loaded with Resources.Load</param>
            public LightIconAttribute(string location)
            {
                this.location = location;
            }

            /// <summary>
            ///     Where Schema should load the light mode icon within a resources folder
            /// </summary>
            /// <param name="location">
            ///     Location of the icon to be loaded with Resources.Load, or if isEditorIcon is true, the name of
            ///     the editor icon to load
            /// </param>
            /// <param name="isEditorIcon">Whether the location specified is the name of an editor icon</param>
            public LightIconAttribute(string location, bool isEditorIcon)
            {
                this.location = location;
                this.isEditorIcon = isEditorIcon;
            }
        }

        /// <summary>
        ///     Attribute for adding a description to a node in the Editor
        /// </summary>
        [AttributeUsage(AttributeTargets.Class)]
        protected class DescriptionAttribute : Attribute
        {
            public string description;

            /// <summary>
            ///     Attribute for adding a description to a node in the Editor
            /// </summary>
            /// <param name="description">Description for the node</param>
            public DescriptionAttribute(string description)
            {
                this.description = description;
            }
        }

        /// <summary>
        ///     Define a custom category for a node
        /// </summary>
        [AttributeUsage(AttributeTargets.Class)]
        protected class CategoryAttribute : Attribute
        {
            public string category;

            /// <summary>
            ///     Define a custom category for a node
            /// </summary>
            /// <param name="category">Content to use for the category in the search menu</param>
            public CategoryAttribute(string category)
            {
                this.category = category;
            }
        }
#if UNITY_EDITOR
        /// <summary>
        ///     Get the icon for a specified type
        /// </summary>
        /// <typeparam name="T">The type of the icon</typeparam>
        /// <returns>The texture for the specified type</returns>
        public static Texture2D GetIcon<T>() where T : GraphObject
        {
            return GetIcon(typeof(T));
        }

        /// <summary>
        ///     Get the icon for a specified type
        /// </summary>
        /// <param name="type">The type of the icon</param>
        /// <returns>The texture for the specified type</returns>
        public static Texture2D GetIcon(Type type)
        {
            if (!typeof(GraphObject).IsAssignableFrom(type))
                throw new ArgumentException("Type parameter does not inherit from GraphObject");

            DarkIconAttribute darkIcon = type.GetCustomAttribute<DarkIconAttribute>();
            LightIconAttribute lightIcon = type.GetCustomAttribute<LightIconAttribute>();

            //Use dark texture
            if (EditorGUIUtility.isProSkin && darkIcon != null)
            {
                Texture2D ret = darkIcon.isEditorIcon
                    ? (Texture2D)EditorGUIUtility.IconContent(darkIcon.location).image
                    : Resources.Load<Texture2D>(darkIcon.location);

                return ret;
            }

            if (lightIcon != null)
            {
                Texture2D ret = lightIcon.isEditorIcon
                    ? (Texture2D)EditorGUIUtility.IconContent(lightIcon.location).image
                    : Resources.Load<Texture2D>(lightIcon.location);

                return ret;
            }

            return null;
        }

        public void ResetGUID()
        {
            m_uID = Guid.NewGuid().ToString("N");
        }
#endif
    }
}