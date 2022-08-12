using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("d_Animator Icon", true)]
    [LightIcon("Animator Icon", true)]
    [Category("Animation")]
    [Description("Set the look at position for an animator")]
    public class SetAnimatorLookAt : Action
    {
        [Tooltip("The position to look at")] public Vector3 lookAtPosition;

        public override void OnInitialize(object nodeMemory, SchemaAgent agent)
        {
            SetAnimatorLayerWeightMemory memory = (SetAnimatorLayerWeightMemory)nodeMemory;

            memory.animator = agent.GetComponent<Animator>();
        }

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            SetAnimatorLayerWeightMemory memory = (SetAnimatorLayerWeightMemory)nodeMemory;

            memory.animator.SetLookAtPosition(lookAtPosition);

            return NodeStatus.Success;
        }

        private class SetAnimatorLayerWeightMemory
        {
            public Animator animator;
        }
    }
}