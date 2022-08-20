using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [Category("Blackboard")]
    public class SetString : Action
    {
        [Tooltip("Value to use when setting")] public BlackboardEntrySelector<string> value;

        [Tooltip("Entry value to set"), WriteOnly] 
        public BlackboardEntrySelector<string> selector;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            selector.value = value.value;

            return NodeStatus.Success;
        }
    }
}