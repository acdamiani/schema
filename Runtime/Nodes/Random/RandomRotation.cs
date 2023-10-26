using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("d_random"), LightIcon("random"), Category("Random"), Description("Get a random rotation")]
    public class RandomRotation : Action
    {
        [Tooltip("Where to store the random rotation"), WriteOnly] 
        public BlackboardEntrySelector<Quaternion> target;

        [Tooltip("Whether to generate a uniformly random rotation")]
        public bool uniform;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            target.value = uniform ? Random.rotationUniform : Random.rotation;

            return NodeStatus.Success;
        }
    }
}