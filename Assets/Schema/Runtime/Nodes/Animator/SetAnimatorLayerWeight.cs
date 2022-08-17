using Schema;
using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("c_Animator")]
    [LightIcon("c_Animator")]
    [Category("Animator")]
    public class SetAnimatorLayerWeight : Action
    {
        public ComponentSelector<Animator> animator;
        [Tooltip("The index of the layer")]
        [Min(0)]
        public int layerIndex;

        [Tooltip("The weight of the layer to set")]
        [Range(0f, 1f)]
        public float layerWeight;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            Animator a = agent.GetComponent(animator);

            if (a == null)
                return NodeStatus.Failure;

            a.SetLayerWeight(layerIndex, layerWeight);

            return NodeStatus.Success;
        }
    }
}