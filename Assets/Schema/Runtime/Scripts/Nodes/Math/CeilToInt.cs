using UnityEngine;
using Schema;
using System.Collections.Generic;
using System.Linq;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("d_math")]
    [LightIcon("math")]
    [Category("Math")]
    [Description("Get the smallest integer greater than or equal to a float")]
    public class CeilToInt : Action
    {
        [Tooltip("Float to ceil")] public BlackboardEntrySelector<float> value;
        [Tooltip("Selector to store the ceiled value in"), WriteOnly] public BlackboardEntrySelector<int> result;
        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            result.value = Mathf.CeilToInt(value.value);

            return NodeStatus.Success;
        }
    }
}