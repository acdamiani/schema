using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("d_Animator Icon", true), LightIcon("Animator Icon", true), Name("Set Animator IK Rotation"),
     Category("Animator")]
    public class SetAnimatorIKRotation : Action
    {
        public ComponentSelector<Animator> animator;
        public AvatarIKGoal goal;
        public BlackboardEntrySelector<Quaternion> goalRotation;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            Animator a = agent.GetComponent(animator);

            if (a == null)
                return NodeStatus.Failure;

            a.SetIKRotation(goal, goalRotation.value);

            return NodeStatus.Success;
        }
    }
}