using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Schema;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("c_Transform")]
    [LightIcon("c_Transform")]
    [Description("Get the inverse of a quaternion rotation")]
    public class QuaternionInverse : Action
    {
        [Tooltip("Rotation to invert")]
        public BlackboardEntrySelector<Quaternion> quaternion;
        [Tooltip("Blackboard variable to store the inverted rotation in"), WriteOnly]
        public BlackboardEntrySelector<Quaternion> inverted;
        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            inverted.value = Quaternion.Inverse(quaternion.value);

            return NodeStatus.Success;
        }
    }
}