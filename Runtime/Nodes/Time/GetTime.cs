using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("Nodes/d_Time"), LightIcon("Nodes/Time"), Description("Get Time.time and store it in a variable")]
    public class GetTime : Action
    {
        [Tooltip("The blackboard variable to store time in"), WriteOnly] 
        public BlackboardEntrySelector<float> time;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            time.value = Time.time;

            return NodeStatus.Success;
        }
    }
}