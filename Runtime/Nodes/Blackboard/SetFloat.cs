using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [Category("Blackboard")]
    public class SetFloat : Action
    {
        [Tooltip("Value to use when setting")] public BlackboardEntrySelector<float> value;

        [Tooltip("Entry value to set"), WriteOnly] 
        public BlackboardEntrySelector<float> selector;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            selector.value = value.value;

            return NodeStatus.Success;
        }
    }
}