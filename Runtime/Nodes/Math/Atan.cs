using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("Nodes/d_Math"), LightIcon("Nodes/Math"), Category("Math"), Description("Get the arctangent of a value")]
    public class Atan : Action
    {
        [Tooltip("Input for the arctangent function")]
        public BlackboardEntrySelector<float> value;

        [Tooltip("Selector to store the arctangent in"), WriteOnly] 
        public BlackboardEntrySelector<float> result;

        [Tooltip("Return degrees instead of radians")]
        public bool degrees;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            result.value = degrees ? Mathf.Atan(value.value) * Mathf.Rad2Deg : Mathf.Atan(value.value);

            return NodeStatus.Success;
        }
    }
}