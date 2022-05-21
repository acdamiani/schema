using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("d_math")]
    [LightIcon("math")]
    [Category("Math")]
    [Description("Get the square root of a number")]
    public class Sqrt : Action
    {
        [Tooltip("Number to get the square root for")] public BlackboardEntrySelector<float> value;
        [Tooltip("Selector to store the square root in"), WriteOnly] public BlackboardEntrySelector<float> result;
        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            result.value = Mathf.Sqrt(value.value);

            return NodeStatus.Success;
        }
    }
}