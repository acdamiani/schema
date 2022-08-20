using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("d_random"), LightIcon("random"), Category("Random"),
     Description("Get a random float value between 0 and 1")]
    public class RandomValue : Action
    {
        [Tooltip("Where to store the random value"), WriteOnly] 
        public BlackboardEntrySelector<float> target;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            target.value = Random.value;

            return NodeStatus.Success;
        }
    }
}