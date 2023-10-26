using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("d_Animator Icon", true), LightIcon("Animator Icon", true), Name("Set Animator IK Rotation Weight"),
     Category("Animator")]
    public class SetAnimatorIKRotationWeight : Action
    {
        public ComponentSelector<Animator> animator;
        public AvatarIKGoal goal;
        public BlackboardEntrySelector<float> weight;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            Animator a = agent.GetComponent(animator);

            if (a == null)
                return NodeStatus.Failure;

            a.SetIKRotationWeight(goal, weight.value);

            return NodeStatus.Success;
        }
    }
}