using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("d_math")]
    [LightIcon("math")]
    [Category("Math")]
    [Description("Get e raised to a specified power")]
    public class Exp : Action
    {
        [Tooltip("Power to raise e to")] public BlackboardEntrySelector<float> pow;

        [Tooltip("Selector to store the cosine in")] [WriteOnly]
        public BlackboardEntrySelector<float> result;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            result.value = Mathf.Exp(pow.value);

            return NodeStatus.Success;
        }
    }
}