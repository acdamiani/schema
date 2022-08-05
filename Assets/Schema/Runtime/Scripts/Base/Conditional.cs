using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.Linq;
using Schema.Internal;

namespace Schema
{
    /// <summary>
    /// Base class for all Schema conditionals
    /// </summary>
    public abstract class Conditional : GraphObject
    {
        /// <summary>
        /// Node that this conditional is attached to
        /// </summary>
        public Node node { get { return m_node; } set { m_node = value; } }
        [SerializeField, HideInInspector] private Node m_node;
        public AbortsType abortsType { get { return m_abortsType; } set { m_abortsType = value; } }
        [SerializeField, HideInInspector] private AbortsType m_abortsType;
        /// <summary>
        /// Invert the condition (a result of false will run the node, a result of true will not)
        /// </summary>
        public bool invert { get { return m_invert; } set { m_invert = value; } }
        [SerializeField, HideInInspector] private bool m_invert;
        /// <summary>
        /// Evaluate this conditional
        /// </summary>
        /// <param name="conditionalMemory">Object containing the memory for the conditional</param>
        /// <param name="agent">Agent executing thsi conditional</param>
        /// <returns></returns>
        public abstract bool Evaluate(object conditionalMemory, SchemaAgent agent);
        /// <summary>
        /// Runs once when all conditonals are first initialized. Similar to Start() in a MonoBehavior class
        /// </summary>
        /// <param name="conditionalMemory">Object containing the memory for the conditional</param>
        /// <param name="agent">Agent executing this conditional</param>
        public virtual void OnInitialize(object conditionalMemory, SchemaAgent agent) { }
        /// <summary>
        /// Runs once once before the conditional is first ticked
        /// </summary>
        /// <param name="conditionalMemory">Object containing the memory for the conditional</param>
        /// <param name="agent">Agent executing this conditional</param>
        public virtual void OnNodeEnter(object conditionalMemory, SchemaAgent agent) { }
        /// <summary>
        /// Runs once when the attached node has finished its execution
        /// </summary>
        /// <param name="conditionalMemory">Object containing the memory for the conditional</param>
        /// <param name="agent">Agent executing this conditional</param>
        /// <param name="status">The status that the node executed with</param>
        public virtual void OnNodeExit(object conditionalMemory, SchemaAgent agent, NodeStatus status) { }
        /// <summary>
        /// Runs before the attached node is ticked
        /// </summary>
        /// <param name="conditionalMemory">Object containing the memory for the conditional</param>
        /// <param name="agent">Agent executing this conditional</param>
        public virtual void Tick(object conditionalMemory, SchemaAgent agent) { }
        /// <summary>
        /// Get the text content to be shown in the editor
        /// </summary>
        public virtual GUIContent GetConditionalContent() { return new GUIContent(GetType().Name); }
        /// <summary>
        /// Possible ways the tree will respond to changes in this conditional's state
        /// </summary>
        public enum AbortsType
        {
            None,
            Self,
            LowerPriority,
            Both
        }
#if UNITY_EDITOR
        public static Conditional Instantiate(Conditional conditional)
        {
            Conditional copy = ScriptableObject.Instantiate<Conditional>(conditional);

            copy.name = conditional.name;
            copy.ResetGUID();
            copy.node = null;

            return copy;
        }
#endif
    }
}