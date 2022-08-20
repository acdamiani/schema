using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("Nodes/d_Quaternion"), LightIcon("Nodes/Quaternion"), Category("Quaternion"),
     Description("Linearly interpolate between two rotations, and normalize the result afterwards")]
    public class LerpQuaternion : Action
    {
        [Tooltip("Quaternion A")] public BlackboardEntrySelector<Quaternion> quaternionOne;

        [Tooltip("Quaternion B")] public BlackboardEntrySelector<Quaternion> quaternionTwo;

        [Tooltip("Amount to interpolate by")] public BlackboardEntrySelector<float> t;

        [Tooltip("Whether to clamp the t value")]
        public bool unclamped;

        [Tooltip("Blackboard variable to store the lerped rotation in"), WriteOnly] 
        public BlackboardEntrySelector<Quaternion> lerped;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            if (unclamped)
                lerped.value = Quaternion.LerpUnclamped(quaternionOne.value, quaternionTwo.value, t.value);
            else
                lerped.value = Quaternion.Lerp(quaternionOne.value, quaternionTwo.value, t.value);

            return NodeStatus.Success;
        }
    }
}