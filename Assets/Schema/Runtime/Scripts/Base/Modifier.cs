using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.Linq;

namespace Schema
{
    /// <summary>
    /// Base class for all Schema modifiers
    /// </summary>
    public abstract class Modifier : ScriptableObject
    {
        /// <summary>
        /// Node that this modifier affects
        /// </summary>
        public Node node { get { return m_node; } set { m_node = value; } }
        [SerializeField, HideInInspector] private Node m_node;
        /// <summary>
        /// Whether this modifier is enabled
        /// </summary>
        public bool enabled { get { return m_enabled; } set { m_enabled = value; } }
        [SerializeField, HideInInspector] private bool m_enabled = true;
        /// <summary>
        /// Runs once when all modifiers are first initialized. Similar to Start() in a MonoBehavior class
        /// </summary>
        /// <param name="modifierMemory">Object containing the memory for the modifier</param>
        /// <param name="agent">Agent executing this modifier</param>
        public virtual void OnInitialize(object modifierMemory, SchemaAgent agent) { }
        /// <summary>
        /// Runs once once before the node is first ticked
        /// </summary>
        /// <param name="modifierMemory">Object containing the memory for the modifier</param>
        /// <param name="agent">Agent executing this modifier</param>
        public virtual void OnNodeEnter(object modifierMemory, SchemaAgent agent) { }
        /// <summary>
        /// Runs once when the attached node has finished its execution
        /// </summary>
        /// <param name="modifierMemory">Object containing the memory for the modifier</param>
        /// <param name="agent">Agent executing this modifier</param>
        /// <param name="status">The status that the node executed with</param>
        public virtual void OnNodeExit(object modifierMemory, SchemaAgent agent, NodeStatus status) { }
        /// <summary>
        /// Runs before the attached node is ticked
        /// </summary>
        /// <param name="modifierMemory">Object containing the memory for the modifier</param>
        /// <param name="agent">Agent executing this modifier</param>
        public virtual void Tick(object modifierMemory, SchemaAgent agent) { }
        public enum Message
        {
            /// <summary>
            /// Quit the node immediately with a failure status
            /// </summary>
            QuitWithFailure,
            /// <summary>
            /// Quit the node immediately with a success status
            /// </summary>
            QuitWithSuccess,
            /// <summary>
            /// Immediately restart the node's execution
            /// </summary>
            Repeat,
            /// <summary>
            /// Repeat the node when it has finished executing
            /// </summary>
            WaitAndRepeat,
            /// <summary>
            /// Wait for the node to finish executing, and force a success status
            /// </summary>
            ForceSuccess,
            /// <summary>
            /// Wait for the node to finish executing, and force a failure status
            /// </summary>
            ForceFailure,
        }
        /// <summary>
        /// Send a message to the Schema runtime to change the flow of the behavior tree
        /// </summary>
        /// <param name="message">The message to send</param>
        protected void SendMessage(Message message)
        {

        }
        void OnEnable()
        {
            NameAttribute attribute = GetType().GetCustomAttribute<NameAttribute>();

            if (String.IsNullOrEmpty(name))
                name = attribute != null ? attribute.name : String.Concat(this.GetType().Name.Select(x => Char.IsUpper(x) ? " " + x : x.ToString())).TrimStart(' ');
        }
        /// <summary>
        /// Attribute to override the default name of the modifier in the editor
        /// </summary>
        [System.AttributeUsage(AttributeTargets.Class)]
        protected class NameAttribute : System.Attribute
        {
            public string name;
            /// <summary>
            /// Attribute to override the default name of the modifier in the edit or
            /// </summary>
            /// <param name="name">Default name of the modifier to use</param>
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
#if UNITY_EDITOR
        private Texture2D _icon;
        /// <summary>
        /// Icon of the modifier (editor only)
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
        /// Whether this modifier is expanded in the editor (Editor Only)
        /// </summary>
        public bool expanded { get { return m_expanded; } set { m_expanded = value; } }
        [SerializeField, HideInInspector] private bool m_expanded;
        public static Texture2D GetModifierIcon(Type type)
        {
            DarkIconAttribute darkIcon = type.GetCustomAttribute<DarkIconAttribute>();
            LightIconAttribute lightIcon = type.GetCustomAttribute<LightIconAttribute>();

            Debug.Log(darkIcon);

            //Use dark texture
            if (EditorGUIUtility.isProSkin && darkIcon != null)
            {
                Texture2D ret = darkIcon.isEditorIcon
                    ? (Texture2D)EditorGUIUtility.IconContent(darkIcon.location).image
                    : Resources.Load<Texture2D>(darkIcon.location);

                return ret;
            }
            else if (lightIcon != null)
            {
                Texture2D ret = lightIcon.isEditorIcon
                    ? (Texture2D)EditorGUIUtility.IconContent(lightIcon.location).image
                    : Resources.Load<Texture2D>(lightIcon.location);

                return ret;
            }
            else
            {
                return null;
            }
        }
#endif
    }
}