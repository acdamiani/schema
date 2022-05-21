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
    public class Ceil : Action
    {
        [Tooltip("Float to floor")] public BlackboardEntrySelector<float> value;
        [Tooltip("Selector to store the ceiled value in"), WriteOnly] public BlackboardEntrySelector<float> result;
        [Tooltip("Convert the ceiled value to an integer")] public bool floorToInt = true;
        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            result.value = Mathf.Ceil(value.value);

            return NodeStatus.Success;
        }
    }
}