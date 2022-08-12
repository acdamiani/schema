using Schema;
using UnityEngine;

[Name("Set Animator IK Position")]
[DarkIcon("c_Animator")]
[LightIcon("c_Animator")]
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

        if (isHint)
            a.SetIKHintPosition(hint, goalPosition.value);
        else
            a.SetIKPosition(goal, goalPosition.value);

        return NodeStatus.Success;
    }
}