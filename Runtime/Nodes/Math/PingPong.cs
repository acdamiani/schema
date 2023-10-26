using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("Nodes/d_Math"), LightIcon("Nodes/Math"), Category("Math"),
     Description("Get a value that will increment and decrement between the value 0 and length based on t")]
    public class PingPong : Action
    {
        [Tooltip("T value for PingPong function")]
        public BlackboardEntrySelector<float> t;

        [Tooltip("Length of the PingPong")] public BlackboardEntrySelector<float> length;

        [Tooltip("Sampled perlin value"), WriteOnly] 
        public BlackboardEntrySelector<float> result;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            result.value = Mathf.PingPong(t.value, length.value);

            return NodeStatus.Success;
        }
    }
}