using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("Nodes/d_Math"), LightIcon("Nodes/Math"), Category("Math"),
     Description("Round a number to the nearest integer")]
    public class RoundToInt : Action
    {
        [Tooltip("Value to round")] public BlackboardEntrySelector<float> value;
        [Tooltip("Rounded value"), WriteOnly]  public BlackboardEntrySelector<int> result;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            result.value = Mathf.RoundToInt(value.value);

            return NodeStatus.Success;
        }
    }
}