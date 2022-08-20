using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("Nodes/d_Quaternion"), LightIcon("Nodes/Quaternion"), Category("Quaternion"),
     Description("Rotates from a rotation towards another rotation")]
    public class RotateTowardsQuaternion : Action
    {
        [Tooltip("Quaternion to rotate from")] public BlackboardEntrySelector<Quaternion> from;

        [Tooltip("Quaternion to rotate to")] public BlackboardEntrySelector<Quaternion> to;

        [Tooltip("The maximum angular step. Negative values will rotate in the opposite direction")]
        public BlackboardEntrySelector<float> maxDegreesDelta;

        [Tooltip("Blackboard variable to store the new rotation in"), WriteOnly] 
        public BlackboardEntrySelector<Quaternion> rotated;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            rotated.value = Quaternion.RotateTowards(from.value, to.value, maxDegreesDelta.value);

            return NodeStatus.Success;
        }
    }
}