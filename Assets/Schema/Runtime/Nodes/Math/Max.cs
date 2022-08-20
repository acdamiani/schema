using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("Nodes/d_Math"), LightIcon("Nodes/Math"), Category("Math"),
     Description("Get the maximum of a list of values")]
    public class Max : Action
    {
        [Tooltip("List of values to get the maximum of")]
        public List<BlackboardEntrySelector<float>> values;

        [Tooltip("Selector to store maximum in"), WriteOnly] 
        public BlackboardEntrySelector<float> result;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            result.value = Mathf.Max(values.Select(v => v.value).ToArray());

            return NodeStatus.Success;
        }
    }
}