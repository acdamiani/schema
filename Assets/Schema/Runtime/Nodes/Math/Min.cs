using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("Nodes/d_Math"), LightIcon("Nodes/Math"), Category("Math"),
     Description("Get the minimum of two or more values")]
    public class Min : Action
    {
        [Tooltip("List of values to get the minimum of")]
        public List<BlackboardEntrySelector<float>> values;

        [Tooltip("Selector to store minimum in"), WriteOnly] 
        public BlackboardEntrySelector<float> result = new BlackboardEntrySelector<float>();

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            result.value = Mathf.Min(values.Select(v => v.value).ToArray());

            return NodeStatus.Success;
        }
    }
}