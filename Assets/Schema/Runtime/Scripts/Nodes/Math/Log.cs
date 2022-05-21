using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("d_math")]
    [LightIcon("math")]
    [Category("Math")]
    [Description("Get the logarithm of a value with a specified base")]
    public class Log : Action
    {
        [Tooltip("Value to get the logarithm of")] public BlackboardEntrySelector<float> value;
        [Tooltip("Base of the logarithm")] public BlackboardEntrySelector<float> baseValue = new BlackboardEntrySelector<float>(2f);
        [Tooltip("Selector to store maximum in"), WriteOnly] public BlackboardEntrySelector<float> result;
        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            result.value = Mathf.Log(value.value, baseValue.value);

            return NodeStatus.Success;
        }
    }
}