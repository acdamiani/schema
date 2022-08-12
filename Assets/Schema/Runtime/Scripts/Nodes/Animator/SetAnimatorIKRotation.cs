using Schema;
using UnityEngine;

[DarkIcon("c_Animator")]
[LightIcon("c_Animator")]
[Name("Set Animator IK Rotation")]
public class SetAnimatorIKRotation : Action
{
    public ComponentSelector<Animator> animator;
    public AvatarIKGoal goal;
    public BlackboardEntrySelector<Quaternion> goalRotation;

    public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
    {
        Animator a = agent.GetComponent(animator);

        a.SetIKRotation(goal, goalRotation.value);

        return NodeStatus.Success;
    }
}