using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Schema;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("c_Transform")]
    [LightIcon("c_Transform")]
    [Description("Take the dot product of two vectors")]
    public class VectorDot : Action
    {
        [Tooltip("Vector A")]
        public BlackboardEntrySelector vectorOne = new BlackboardEntrySelector();
        [Tooltip("Vector B")]
        public BlackboardEntrySelector vectorTwo = new BlackboardEntrySelector();
        [Tooltip("Blackboard variable to store the dot product in"), WriteOnly]
        public BlackboardEntrySelector dot = new BlackboardEntrySelector();
        protected override void OnNodeEnable()
        {
            vectorOne.ApplyFilters(typeof(Vector2), typeof(Vector3), typeof(Vector4));
            vectorTwo.ApplyFilters(typeof(Vector2), typeof(Vector3), typeof(Vector4));
            dot.ApplyFilter<float>();
        }
        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            dot.value = Vector4.Dot((Vector4)vectorOne.value, (Vector4)vectorTwo.value);

            return NodeStatus.Success;
        }
    }
}