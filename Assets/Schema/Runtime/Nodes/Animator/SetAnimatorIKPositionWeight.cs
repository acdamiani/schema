using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [Name("Set Animator IK Position Weight"), DarkIcon("d_Animator Icon", true), LightIcon("Animator Icon", true),
     Category("Animator")]
    public class SetAnimatorIKPositionWeight : Action
    {
        public ComponentSelector<Animator> animator;
        public AvatarIKGoal goal;
        public AvatarIKHint hint;
        public bool isHint;
        public BlackboardEntrySelector<float> weight;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            Animator a = agent.GetComponent(animator);

            if (a == null)
                return NodeStatus.Failure;

            if (isHint)
                a.SetIKHintPositionWeight(hint, weight.value);
            else
                a.SetIKPositionWeight(goal, weight.value);

            return NodeStatus.Success;
        }
    }
}