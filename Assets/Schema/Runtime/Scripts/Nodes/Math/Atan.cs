using UnityEngine;
using Schema;
using System.Collections.Generic;
using System.Linq;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("d_math")]
    [LightIcon("math")]
    [Category("Math")]
    [Description("Get the arctangent of a value")]
    public class Atan : Action
    {
        [Tooltip("Input for the arctangent function")] public BlackboardEntrySelector<float> value;
        [Tooltip("Selector to store the arctangent in"), WriteOnly] public BlackboardEntrySelector<float> result;
        [Tooltip("Return degrees instead of radians")] public bool degrees;
        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            result.value = degrees ? Mathf.Atan(value.value) * Mathf.Rad2Deg : Mathf.Atan(value.value);

            return NodeStatus.Success;
        }
    }
}