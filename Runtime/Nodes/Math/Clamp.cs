using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("Nodes/d_Math"), LightIcon("Nodes/Math"), Category("Math"),
     Description("Clamp a float to be between two other float values")]
    public class Clamp : Action
    {
        [Tooltip("Float to clamp")] public BlackboardEntrySelector<float> value;
        [Tooltip("Lower bound for the float")] public BlackboardEntrySelector<float> lowerBound;
        [Tooltip("Upper bound for the float")] public BlackboardEntrySelector<float> upperBound;

        [Tooltip("The clamped float"), WriteOnly] 
        public BlackboardEntrySelector<float> result;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            result.value = Mathf.Clamp(value.value, lowerBound.value, upperBound.value);

            return NodeStatus.Success;
        }
    }
}