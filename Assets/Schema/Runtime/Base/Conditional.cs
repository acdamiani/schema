using Schema.Internal;
using UnityEngine;

namespace Schema
{
    /// <summary>
    ///     Base class for all Schema conditionals
    /// </summary>
    public abstract class Conditional : GraphObject
    {
        /// <summary>
        ///     Describes how nodes can be aborted by the conditional
        /// </summary>
        public enum AbortsType
        {
            None,
            Self,
            LowerPriority,
            Both
        }

        /// <summary>
        ///     When the conditional should abort other actions
        /// </summary>
        public enum AbortsWhen
        {
            OnSuccess,
            OnFailure,
            Both
        }

        [SerializeField, HideInInspector]  private Node m_node;
        [SerializeField, HideInInspector]  private AbortsType m_abortsType;
        [SerializeField, HideInInspector]  private AbortsWhen m_abortsWhen;
        [SerializeField, HideInInspector]  private bool m_invert;

        /// <summary>
        ///     Node that this conditional is attached to
        /// </summary>
        public Node node
        {
            get => m_node;
            set => m_node = value;
        }

        public AbortsType abortsType
        {
            get => m_abortsType;
            set => m_abortsType = value;
        }

        public AbortsWhen abortsWhen
        {
            get => m_abortsWhen;
            set => m_abortsWhen = value;
        }

        /// <summary>
        ///     Invert the condition (a result of false will run the node, a result of true will not)
        /// </summary>
        public bool invert
        {
            get => m_invert;
            set => m_invert = value;
        }

        /// <summary>
        ///     Evaluate this conditional
        /// </summary>
        /// <param name="conditionalMemory">Object containing the memory for the conditional</param>
        /// <param name="agent">Agent executing thsi conditional</param>
        /// <returns></returns>
        public abstract bool Evaluate(object conditionalMemory, SchemaAgent agent);

        /// <summary>
        ///     Runs once when all conditonals are first initialized. Similar to Start() in a MonoBehavior class
        /// </summary>
        /// <param name="conditionalMemory">Object containing the memory for the conditional</param>
        /// <param name="agent">Agent executing this conditional</param>
        public virtual void OnInitialize(object conditionalMemory, SchemaAgent agent)
        {
        }

        /// <summary>
        ///     Override this method to draw gizmos for this node in the Scene view
        /// </summary>
        public virtual void DoConditionalGizmos(SchemaAgent agent)
        {
        }

        /// <summary>
        ///     Get the text content to be shown in the editor
        /// </summary>
        public virtual GUIContent GetConditionalContent()
        {
            return new GUIContent(name);
        }
        #if UNITY_EDITOR
        public static Conditional Instantiate(Conditional conditional)
        {
            Conditional copy = ScriptableObject.Instantiate(conditional);

            copy.name = conditional.name;
            copy.ResetGUID();
            copy.node = null;

            return copy;
        }
        #endif
    }
}