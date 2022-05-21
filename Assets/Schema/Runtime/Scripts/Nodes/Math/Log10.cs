using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("d_math")]
    [LightIcon("math")]
    [Category("Math")]
    [Description("Get the logarithm of a value with base 10")]
    public class Log10 : Action
    {
        [Tooltip("Value to get the logarithm of")] public BlackboardEntrySelector<float> value;
        [Tooltip("Selector to store maximum in"), WriteOnly] public BlackboardEntrySelector<float> result;
        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            result.value = Mathf.Log10(value.value);

            return NodeStatus.Success;
        }
    }
}