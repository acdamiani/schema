using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("Nodes/d_Math"), LightIcon("Nodes/Math"), Category("Math"),
     Description("Get the arctangent of a ratio y/x")]
    public class Atan2 : Action
    {
        [Tooltip("Numerator of the ratio")] public BlackboardEntrySelector<float> y;
        [Tooltip("Denominator of the ratio")] public BlackboardEntrySelector<float> x;

        [Tooltip("Selector to store the arctangent in"), WriteOnly] 
        public BlackboardEntrySelector<float> result;

        [Tooltip("Return degrees instead of radians")]
        public bool degrees;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            result.value = degrees ? Mathf.Atan2(y.value, x.value) * Mathf.Rad2Deg : Mathf.Atan2(y.value, x.value);

            return NodeStatus.Success;
        }
    }
}