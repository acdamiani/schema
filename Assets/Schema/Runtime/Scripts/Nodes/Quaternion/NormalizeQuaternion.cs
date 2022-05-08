using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Schema;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("c_Transform")]
    [LightIcon("c_Transform")]
    [Description("Normalize a Quaternion")]
    public class NormalizeQuaternion : Action
    {
        [Tooltip("Quaternion to normalize")]
        public BlackboardEntrySelector<Quaternion> quaternion;
        [Tooltip("Blackboard variable to store the normalized vector in"), WriteOnly]
        public BlackboardEntrySelector<Quaternion> normalized;
        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            normalized.value = Quaternion.Normalize(quaternion.value);

            return NodeStatus.Success;
        }
    }
}