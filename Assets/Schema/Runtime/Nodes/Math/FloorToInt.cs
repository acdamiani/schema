using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("Nodes/d_Math"), LightIcon("Nodes/Math"), Category("Math"),
     Description("Get the greatest integer less than or equal to a float")]
    public class FloorToInt : Action
    {
        [Tooltip("Float to floor")] public BlackboardEntrySelector<float> value;

        [Tooltip("Selector to store the floored value in"), WriteOnly] 
        public BlackboardEntrySelector<int> result;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            result.value = Mathf.FloorToInt(value.value);

            return NodeStatus.Success;
        }
    }
}