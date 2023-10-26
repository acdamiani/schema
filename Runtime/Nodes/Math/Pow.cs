using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("Nodes/d_Math"), LightIcon("Nodes/Math"), Category("Math"),
     Description("Get a number raised to a specified power")]
    public class Pow : Action
    {
        [Tooltip("Float to exponentiate")] public BlackboardEntrySelector<float> value;

        [Tooltip("Power to raise the float to")]
        public BlackboardEntrySelector<float> pow;

        [Tooltip("Selector to store the exponentiated value in"), WriteOnly] 
        public BlackboardEntrySelector<float> result;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            result.value = Mathf.Pow(value.value, pow.value);

            return NodeStatus.Success;
        }
    }
}