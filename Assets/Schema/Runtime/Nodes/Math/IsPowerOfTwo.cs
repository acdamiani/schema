using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("Nodes/d_Math"), LightIcon("Nodes/Math"), Category("Math"),
     Description("Determine if an integer is a power of two")]
    public class IsPowerOfTwo : Action
    {
        [Tooltip("Integer to evaluate")] public BlackboardEntrySelector<int> value;

        [Tooltip("Whether the integer is a power of two"), WriteOnly] 
        public BlackboardEntrySelector<bool> result;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            result.value = Mathf.IsPowerOfTwo(value.value);

            return NodeStatus.Success;
        }
    }
}