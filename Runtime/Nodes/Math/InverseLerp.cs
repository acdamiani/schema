using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("Nodes/d_Math"), LightIcon("Nodes/Math"), Category("Math"),
     Description("Get a float's position in a range")]
    public class InverseLerp : Action
    {
        [Tooltip("Float to use to determine position")]
        public BlackboardEntrySelector<float> value;

        [Tooltip("Lower value of the range")] public BlackboardEntrySelector<float> lowerBound;
        [Tooltip("Upper value of the range")] public BlackboardEntrySelector<float> upperBound;

        [Tooltip("Position of the float in the range (between 0 and 1)"), WriteOnly] 
        public BlackboardEntrySelector<float> result;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            result.value = Mathf.InverseLerp(lowerBound.value, upperBound.value, value.value);

            return NodeStatus.Success;
        }
    }
}