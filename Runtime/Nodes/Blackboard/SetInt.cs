using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [Category("Blackboard")]
    public class SetInt : Action
    {
        [Tooltip("Value to use when setting")] public BlackboardEntrySelector<int> value;

        [Tooltip("Entry value to set"), WriteOnly] 
        public BlackboardEntrySelector<int> selector;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            selector.value = value.value;

            return NodeStatus.Success;
        }
    }
}