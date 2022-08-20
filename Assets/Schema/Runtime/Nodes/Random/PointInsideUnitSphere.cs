using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("d_random"), LightIcon("random"), Category("Random"),
     Description("Get a random point inside a unit sphere with radius 1.0")]
    public class PointInsideUnitSphere : Action
    {
        [Tooltip("Where to store the random point"), WriteOnly] 
        public BlackboardEntrySelector<Vector3> target;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            target.value = Random.insideUnitSphere;

            return NodeStatus.Success;
        }
    }
}