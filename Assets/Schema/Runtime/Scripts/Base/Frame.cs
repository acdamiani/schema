using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.Linq;

namespace Schema
{
    /// <summary>
    /// Frame component
    /// </summary>
    public sealed class Frame : ScriptableObject
    {
        /// <summary>
        /// Objects that this frame affects
        /// </summary>
        public UnityEngine.Object[] children { get { return m_children; } set { m_children = value; } }
        [SerializeField, HideInInspector] private UnityEngine.Object[] m_children;
        /// <summary>
        /// Whether to display a custom color for this frame
        /// </summary>
        public bool useCustomColor { get { return m_useCustomColor; } }
        [SerializeField] private bool m_useCustomColor;
        /// <summary>
        /// The custom color for this frame
        /// </summary>
        public Color customColor { get { return m_customColor; } }
        [SerializeField] private Color m_customColor = new Color(0.05f, 0.05f, 0.05f);
    }
}