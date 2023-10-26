using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("Nodes/d_Math"), LightIcon("Nodes/Math"), Category("Math"),
     Description("Get the logarithm of a value with base 10")]
    public class Log10 : Action
    {
        [Tooltip("Value to get the logarithm of")]
        public BlackboardEntrySelector<float> value;

        [Tooltip("Selector to store maximum in"), WriteOnly] 
        public BlackboardEntrySelector<float> result;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            result.value = Mathf.Log10(value.value);

            return NodeStatus.Success;
        }
    }
}