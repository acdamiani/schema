using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("Nodes/d_Math"), LightIcon("Nodes/Math"), Category("Math"), Description("Get the cosine of an angle")]
    public class Cos : Action
    {
        [Tooltip("Input for the cosine function")]
        public BlackboardEntrySelector<float> value;

        [Tooltip("Selector to store the cosine in"), WriteOnly] 
        public BlackboardEntrySelector<float> result;

        [Tooltip("Input is degrees instead of radians")]
        public bool degrees;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            float angle = degrees ? value.value * Mathf.Deg2Rad : value.value;

            result.value = Mathf.Cos(angle);

            return NodeStatus.Success;
        }
    }
}