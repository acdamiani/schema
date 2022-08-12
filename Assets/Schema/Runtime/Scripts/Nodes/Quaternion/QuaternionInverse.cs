using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("d_Transform Icon", true)]
    [LightIcon("Transform Icon", true)]
    [Description("Get the inverse of a quaternion rotation")]
    public class QuaternionInverse : Action
    {
        [Tooltip("Rotation to invert")] public BlackboardEntrySelector<Quaternion> quaternion;

        [Tooltip("Blackboard variable to store the inverted rotation in")] [WriteOnly]
        public BlackboardEntrySelector<Quaternion> inverted;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            inverted.value = Quaternion.Inverse(quaternion.value);

            return NodeStatus.Success;
        }
    }
}