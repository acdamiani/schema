using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Schema
{
    /// <summary>
    ///     Base class for all Schema modifiers
    /// </summary>
    public abstract class Modifier : ScriptableObject
    {
        public enum Message
        {
            /// <summary>
            ///     Do nothing
            /// </summary>
            None,

            /// <summary>
            ///     Restart the node's execution
            /// </summary>
            Repeat,

            /// <summary>
            ///     Wait for the node to finish executing, and force a success status
            /// </summary>
            ForceSuccess,

            /// <summary>
            ///     Wait for the node to finish executing, and force a failure status
            /// </summary>
            ForceFailure
        }

        [SerializeField, HideInInspector]  private Node m_node;
        [SerializeField, HideInInspector]  private bool m_enabled = true;

        /// <summary>
        ///     Node that this modifier affects
        /// </summary>
        public Node node
        {
            get => m_node;
            set => m_node = value;
        }

        /// <summary>
        ///     Whether this modifier is enabled
        /// </summary>
        public bool enabled
        {
            get => m_enabled;
            set => m_enabled = value;
        }

        private void OnEnable()
        {
            NameAttribute attribute = GetType().GetCustomAttribute<NameAttribute>();

            if (string.IsNullOrEmpty(name))
                name = attribute != null
                    ? attribute.name
                    : string.Concat(GetType().Name.Select(x => char.IsUpper(x) ? " " + x : x.ToString()))
                        .TrimStart(' ');
        }

        /// <summary>
        ///     Runs once when all modifiers are first initialized. Similar to Start() in a MonoBehavior class
        /// </summary>
        /// <param name="modifierMemory">Object containing the memory for the modifier</param>
        /// <param name="agent">Agent executing this modifier</param>
        public virtual void OnInitialize(object modifierMemory, SchemaAgent agent)
        {
        }

        /// <summary>
        ///     Runs once when the attached node has finished its execution. Where you can modify the state of the tree
        /// </summary>
        /// <param name="modifierMemory">Object containing the memory for the modifier</param>
        /// <param name="agent">Agent executing this modifier</param>
        /// <param name="status">The status that the node executed with</param>
        public virtual Message Modify(object modifierMemory, SchemaAgent agent, NodeStatus status)
        {
            return Message.None;
        }

        public static IEnumerable<Type> DisallowedTypes(Type type)
        {
            DisableIfTypesAttribute attribute = type.GetCustomAttribute<DisableIfTypesAttribute>();

            if (attribute == null)
                return Enumerable.Empty<Type>();
            return attribute.types;
        }

        public static bool AllowedOne(Type type)
        {
            return type.GetCustomAttribute<AllowOneAttribute>() != null;
        }

        /// <summary>
        ///     Allow only one of these modifiers to be attached to a node at a given time
        /// </summary>
        [AttributeUsage(AttributeTargets.Class)]
        protected class AllowOneAttribute : Attribute
        {
        }

        /// <summary>
        ///     Disable adding this modifier if any of the specified types are present
        /// </summary>
        protected class DisableIfTypesAttribute : Attribute
        {
            public IEnumerable<Type> types;

            /// <summary>
            ///     Allow only one of these modifiers to be attached to a node at a given time
            /// </summary>
            public DisableIfTypesAttribute(params Type[] types)
            {
                this.types = types
                    .Where(x => typeof(Modifier).IsAssignableFrom(x));
            }
        }

        /// <summary>
        ///     Attribute to override the default name of the modifier in the editor
        /// </summary>
        [AttributeUsage(AttributeTargets.Class)]
        protected class NameAttribute : Attribute
        {
            public string name;

            /// <summary>
            ///     Attribute to override the default name of the modifier in the edit or
            /// </summary>
            /// <param name="name">Default name of the modifier to use</param>
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
        #if UNITY_EDITOR
        public static Modifier Instantiate(Modifier modifier)
        {
            Modifier copy = ScriptableObject.Instantiate(modifier);

            copy.name = modifier.name;
            copy.node = null;

            return copy;
        }

        private Texture2D _icon;

        /// <summary>
        ///     Icon of the modifier (editor only)
        /// </summary>
        public Texture2D icon
        {
            get
            {
                if (usingProSkin != EditorGUIUtility.isProSkin)
                {
                    _icon = GetModifierIcon(GetType());
                    usingProSkin = EditorGUIUtility.isProSkin;
                }

                return _icon;
            }
        }

        private bool usingProSkin;

        /// <summary>
        ///     Whether this modifier is expanded in the editor (Editor Only)
        /// </summary>
        public bool expanded
        {
            get => m_expanded;
            set => m_expanded = value;
        }

        [SerializeField, HideInInspector]  private bool m_expanded;
        public static Texture2D GetModifierIcon(Type type)
        {
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
        #endif
    }
}