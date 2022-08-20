using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("Nodes/d_Math"), LightIcon("Nodes/Math"), Category("Math"),
     Description("Round a number to the nearest integer")]
    public class Round : Action
    {
        [Tooltip("Value to round")] public BlackboardEntrySelector<float> value;

        [Tooltip("Sampled perlin value"), WriteOnly] 
        public BlackboardEntrySelector<float> result;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            result.value = Mathf.Round(value.value);

            return NodeStatus.Success;
        }
    }
}