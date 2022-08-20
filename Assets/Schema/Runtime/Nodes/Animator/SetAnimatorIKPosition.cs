using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [Name("Set Animator IK Position"), DarkIcon("d_Animator Icon", true), LightIcon("Animator Icon", true),
     Category("Animator")]
    public class SetAnimatorIKPosition : Action
    {
        public ComponentSelector<Animator> animator;
        public AvatarIKGoal goal;
        public AvatarIKHint hint;
        public bool isHint;
        public BlackboardEntrySelector<Vector3> goalPosition;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            Animator a = agent.GetComponent(animator);

            if (a == null)
                return NodeStatus.Failure;

            if (isHint)
                a.SetIKHintPosition(hint, goalPosition.value);
            else
                a.SetIKPosition(goal, goalPosition.value);

            return NodeStatus.Success;
        }
    }
}