using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [Category("Blackboard")]
    public class SetQuaternion : Action
    {
        [Tooltip("Value to use when setting")] public BlackboardEntrySelector<Quaternion> value;

        [Tooltip("Entry value to set")] [WriteOnly]
        public BlackboardEntrySelector<Quaternion> selector;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            selector.value = value.value;

            return NodeStatus.Success;
        }
    }
}