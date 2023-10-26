using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("Nodes/d_Math"), LightIcon("Nodes/Math"), Category("Math"),
     Description("Get the closest power of two to an integer")]
    public class ClosestPowerOfTwo : Action
    {
        [Tooltip("Input integer")] public BlackboardEntrySelector<int> value;

        [Tooltip("Closest power of two"), WriteOnly] 
        public BlackboardEntrySelector<int> result;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            result.value = Mathf.ClosestPowerOfTwo(value.value);

            return NodeStatus.Success;
        }
    }
}