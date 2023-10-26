using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("Nodes/d_Quaternion"), LightIcon("Nodes/Quaternion"), Category("Quaternion"),
     Description("Spherically interpolate between two rotations, and normalize the result afterwards")]
    public class SlerpQuaternion : Action
    {
        [Tooltip("Quaternion A")] public BlackboardEntrySelector<Quaternion> quaternionOne;

        [Tooltip("Quaternion B")] public BlackboardEntrySelector<Quaternion> quaternionTwo;

        [Tooltip("Amount to interpolate by")] public BlackboardEntrySelector<float> t;

        [Tooltip("Whether to clamp the t value")]
        public bool unclamped;

        [Tooltip("Blackboard variable to store the slerped rotation in"), WriteOnly] 
        public BlackboardEntrySelector<Quaternion> slerped;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            if (unclamped)
                slerped.value = Quaternion.SlerpUnclamped(quaternionOne.value, quaternionTwo.value, t.value);
            else
                slerped.value = Quaternion.Slerp(quaternionOne.value, quaternionTwo.value, t.value);

            return NodeStatus.Success;
        }
    }
}