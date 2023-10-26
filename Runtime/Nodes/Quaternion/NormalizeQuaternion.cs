using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("Nodes/d_Quaternion"), LightIcon("Nodes/Quaternion"), Category("Quaternion"),
     Description("Normalize a Quaternion")]
    public class NormalizeQuaternion : Action
    {
        [Tooltip("Quaternion to normalize")] public BlackboardEntrySelector<Quaternion> quaternion;

        [Tooltip("Blackboard variable to store the normalized vector in"), WriteOnly] 
        public BlackboardEntrySelector<Quaternion> normalized;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            normalized.value = Quaternion.Normalize(quaternion.value);

            return NodeStatus.Success;
        }
    }
}