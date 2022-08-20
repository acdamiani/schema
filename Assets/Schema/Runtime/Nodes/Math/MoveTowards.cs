using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("Nodes/d_Math"), LightIcon("Nodes/Math"), Category("Math"),
     Description("Move a float towards another float by a specified step size")]
    public class MoveTowards : Action
    {
        [Tooltip("Current float")] public BlackboardEntrySelector<float> current;
        [Tooltip("Target float")] public BlackboardEntrySelector<float> target;

        [Tooltip("Max delta to move towards the target")]
        public BlackboardEntrySelector<float> maxDelta;

        [Tooltip("The lerped float"), WriteOnly] 
        public BlackboardEntrySelector<float> result;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            result.value = Mathf.MoveTowards(current.value, target.value, maxDelta.value);

            return NodeStatus.Success;
        }
    }
}