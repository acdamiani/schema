using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("Nodes/d_Time"), LightIcon("Nodes/Time"), Description("Get Time.deltaTime and store it in a variable")]
    public class GetDeltaTime : Action
    {
        [Tooltip("The blackboard variable to store deltaTime in"), WriteOnly] 
        private BlackboardEntrySelector<float> deltaTime;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            deltaTime.value = Time.deltaTime;

            return NodeStatus.Success;
        }
    }
}