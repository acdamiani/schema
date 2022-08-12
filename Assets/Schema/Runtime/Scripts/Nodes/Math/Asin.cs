using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("d_math")]
    [LightIcon("math")]
    [Category("Math")]
    [Description("Get the arcsine of a value")]
    public class Asin : Action
    {
        [Tooltip("Input for the arcsine function")]
        public BlackboardEntrySelector<float> value;

        [Tooltip("Selector to store the arcsine in")] [WriteOnly]
        public BlackboardEntrySelector<float> result;

        [Tooltip("Return degrees instead of radians")]
        public bool degrees;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            result.value = degrees ? Mathf.Asin(value.value) * Mathf.Rad2Deg : Mathf.Asin(value.value);

            return NodeStatus.Success;
        }
    }
}