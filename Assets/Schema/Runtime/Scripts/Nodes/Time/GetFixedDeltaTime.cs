using UnityEngine;

namespace Schema.Builtin.Nodes
{
    public class GetFixedDeltaTime : Action
    {
        [Tooltip("The blackboard variable to store fixedDeltaTime in")] [WriteOnly]
        private BlackboardEntrySelector<float> fixedDeltaTime;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            fixedDeltaTime.value = Time.fixedDeltaTime;

            return NodeStatus.Success;
        }
    }
}