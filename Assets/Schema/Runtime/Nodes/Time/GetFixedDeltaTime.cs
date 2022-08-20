using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("Nodes/d_Time"), LightIcon("Nodes/Time")]
    public class GetFixedDeltaTime : Action
    {
        [Tooltip("The blackboard variable to store fixedDeltaTime in"), WriteOnly] 
        private BlackboardEntrySelector<float> fixedDeltaTime;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            fixedDeltaTime.value = Time.fixedDeltaTime;

            return NodeStatus.Success;
        }
    }
}