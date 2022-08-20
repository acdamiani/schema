using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("Nodes/d_Math"), LightIcon("Nodes/Math"), Category("Math"), Description("Get the sign of a number")]
    public class Sign : Action
    {
        [Tooltip("Value to get the sign of")] public BlackboardEntrySelector<float> value;

        [Tooltip("Sign of the value"), WriteOnly] 
        public BlackboardEntrySelector<float> result;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            result.value = Mathf.Sign(value.value);

            return NodeStatus.Success;
        }
    }
}