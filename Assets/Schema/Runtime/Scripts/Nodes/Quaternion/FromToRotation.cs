using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Schema;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("c_Transform")]
    [LightIcon("c_Transform")]
    [Description("Creates a rotation which rotates from one direction to another direction")]
    public class FromToRotation : Action
    {
        [Tooltip("Direction to rotate from")]
        public BlackboardEntrySelector<Vector3> fromDirection;
        [Tooltip("Direction to rotate to")]
        public BlackboardEntrySelector<Vector3> toDirection;
        [Tooltip("Blackboard variable to store the new rotation in"), WriteOnly]
        public BlackboardEntrySelector<Quaternion> rotated;
        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            rotated.value = Quaternion.FromToRotation(fromDirection.value, toDirection.value);

            return NodeStatus.Success;
        }
    }
}