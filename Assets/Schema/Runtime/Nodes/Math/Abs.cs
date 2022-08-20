using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("Nodes/d_Math"), LightIcon("Nodes/Math"), Category("Math"),
     Description("Get the absolute value of a float")]
    public class Abs : Action
    {
        [Tooltip("Value to get the absolute value of")]
        public BlackboardEntrySelector<float> value;

        [Tooltip("Selector to store the absolute value in"), WriteOnly] 
        public BlackboardEntrySelector<float> result;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            result.value = Mathf.Abs(value.value);

            return NodeStatus.Success;
        }
    }
}