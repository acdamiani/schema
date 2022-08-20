using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("Nodes/d_Math"), LightIcon("Nodes/Math"), Category("Math"),
     Description("Get the natural logarithm of a value (with base e)")]
    public class NaturalLog : Action
    {
        [Tooltip("Value to get the logarithm of")]
        public BlackboardEntrySelector<float> value;

        [Tooltip("Selector to store maximum in"), WriteOnly] 
        public BlackboardEntrySelector<float> result;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            result.value = Mathf.Log(value.value);

            return NodeStatus.Success;
        }
    }
}