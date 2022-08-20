using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("Nodes/d_Math"), LightIcon("Nodes/Math"), Category("Math"),
     Description("Get the next power of two greater than or equal to an integer")]
    public class NextPowerOfTwo : Action
    {
        [Tooltip("Input integer")] public BlackboardEntrySelector<int> value;

        [Tooltip("Next power of two"), WriteOnly] 
        public BlackboardEntrySelector<int> result;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            result.value = Mathf.NextPowerOfTwo(value.value);

            return NodeStatus.Success;
        }
    }
}