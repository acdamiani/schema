using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("d_Animator Icon", true), LightIcon("Animator Icon", true), Category("Animator"),
     Description("Set the look at position for an animator")]
    public class SetAnimatorLookAt : Action
    {
        public ComponentSelector<Animator> animator;
        [Tooltip("The position to look at")] public Vector3 lookAtPosition;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            Animator a = agent.GetComponent(animator);

            if (a == null)
                return NodeStatus.Failure;

            a.SetLookAtPosition(lookAtPosition);

            return NodeStatus.Success;
        }
    }
}