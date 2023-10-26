using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [Category("Blackboard")]
    public class SetVector4 : Action
    {
        [Tooltip("Value to use when setting")] public BlackboardEntrySelector<Vector4> value;

        [Tooltip("Entry value to set"), WriteOnly] 
        public BlackboardEntrySelector<Vector4> selector;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            selector.value = value.value;

            return NodeStatus.Success;
        }
    }
}