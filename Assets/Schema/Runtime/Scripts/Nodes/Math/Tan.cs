
using UnityEngine;
using Schema;
using System.Collections.Generic;
using System.Linq;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("d_math")]
    [LightIcon("math")]
    [Category("Math")]
    [Description("Get the tangent of an angle")]
    public class Tan : Action
    {
        [Tooltip("Input for the tangent function")] public BlackboardEntrySelector<float> value;
        [Tooltip("Selector to store the tangent in"), WriteOnly] public BlackboardEntrySelector<float> result;
        [Tooltip("Input is degrees instead of radians")] public bool degrees;
        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            float angle = degrees ? value.value * Mathf.Deg2Rad : value.value;

            result.value = Mathf.Tan(angle);

            return NodeStatus.Success;
        }
    }
}