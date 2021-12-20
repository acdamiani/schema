using UnityEngine;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

namespace Schema.Runtime
{
    [System.Serializable]
    public abstract class Decorator : ScriptableObject
    {
        private string _name;
        public string Name
        {
            get
            {
                return String.IsNullOrEmpty(_name) ? this.GetType().ToString() : _name;
            }
            set
            {
                _name = value;
            }
        }
        [HideInInspector] public string uID;
        [NonSerialized] public List<string> info;
        [HideInInspector] public Node node;
        /// <summary>
        ///	Runs when the tree is first initialized, per agent 
        /// </summary>
        /// <param name="decoratorMemory">The memory object of the decorator, which can be safely cast into the decorator's memory type</param>
        /// <param name="agent">The agent calling this method</param>
        public virtual void OnInitialize(object decoratorMemory, SchemaAgent agent) { }
        /// <summary>
        ///	Determines whether the node attached to the decorator will be run. If any decorators on the stack return false, the node will not be run. 
        ///	If not overridden, the decorator will be ignored when evaluating the stack. 
        /// </summary>
        /// <param name="decoratorMemory">The memory object of the decorator, which can be safely cast into the decorator's memory type</param>
        /// <param name="agent">The agent calling this method</param>
        /// <returns>Whether this decorator in the stack will allow the node to run</returns>
        public virtual bool Evaluate(object decoratorMemory, SchemaAgent agent)
        {
            return true;
        }
        /// <summary>
        /// Run when flow enters the node the decorator is attached to
        /// </summary>
        /// <param name="decoratorMemory">The memory object of the decorator, which can be safely cast into the decorator's memory type</param>
        /// <param name="agent">The agent calling this method</param>
        public virtual void OnFlowEnter(object decoratorMemory, SchemaAgent agent) { }
        /// <summary>
        ///	Run when flow exits the node the decorator is attached to 
        /// </summary>
        /// <param name="decoratorMemory">The memory object of the decorator, which can be safely cast into the decorator's memory type</param>
        /// <param name="agent">The agent calling this method</param>
        public virtual void OnFlowExit(object decoratorMemory, SchemaAgent agent) { }
        /// <summary>
        /// Optional override method that will modify the behavior of the tree after a subtree has finished processing (will return a result to its parent).
        /// This method can be overiden to facilitate loops or  modification of state.
        /// </summary>
        /// <param name="decoratorMemory">The memory object of the decorator, which can be safely cast into the decorator's memory type</param>
        /// <param name="agent">The agent calling this method</param>
        /// <param name="status">The status of the node</param>
        /// <returns>Whether to repeat execution of this node instead of returning result to the parent. Note that this will call OnFlowEnter() and OnFlowExit()</returns>
        public virtual bool OnNodeProcessed(object decoratorMemory, SchemaAgent agent, ref NodeStatus status) { return false; }
        /// <summary>
        ///	Override to allow for Gizmo visualization in the inspector. This will be called only for the currently selected SchemaAgent. 
        /// </summary>
        public virtual void DrawGizmos(SchemaAgent agent) { }
        /// <summary>
        ///	Get all errors for the object, to display inside the editor 
        /// </summary>
        /// <returns>The list of errors to display</returns>
        public virtual List<Error> GetErrors() { return new List<Error>(); }
        public Decorator()
        {
            if (string.IsNullOrEmpty(uID)) uID = Guid.NewGuid().ToString("N");
        }
        public bool isConditional
        {
            get
            {
                if (_isConditional.HasValue)
                {
                    return (bool)_isConditional;
                }
                else
                {
                    _isConditional = GetType().GetMethod("Evaluate").DeclaringType == GetType();
                    return (bool)_isConditional;
                }
            }
        }
        private bool? _isConditional;
        public bool allowOnlyOne
        {
            get
            {
                if (_allowOnlyOne.HasValue)
                {
                    return (bool)_allowOnlyOne;
                }
                else
                {
                    _allowOnlyOne = GetType().IsDefined(typeof(AllowOnlyOneAttribute));
                    return (bool)_allowOnlyOne;
                }
            }
        }
        private bool? _allowOnlyOne;
        [HideInInspector] public ObserverAborts abortsType;
        public enum ObserverAborts
        {
            None,
            Self,
            LowerPriority,
            Both
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

        [System.AttributeUsage(System.AttributeTargets.Field | System.AttributeTargets.Property)]
        public class InfoAttribute : System.Attribute { }

        public bool IsInfoAttributeDefined(FieldInfo field)
        {
            return field.IsDefined(typeof(InfoAttribute));
        }

        public bool IsInfoAttributeDefined(PropertyInfo property)
        {
            return property.IsDefined(typeof(InfoAttribute));
        }
        [System.AttributeUsage(System.AttributeTargets.Class)]
        public class AllowOnlyOneAttribute : System.Attribute { }
    }
}