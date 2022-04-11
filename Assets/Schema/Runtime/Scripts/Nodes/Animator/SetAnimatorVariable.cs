using Schema;
using UnityEngine;

[RequireAgentComponent(typeof(Animator))]
[DarkIcon("Dark/SetAnimatorVariable")]
[LightIcon("Light/SetAnimatorVariable")]
public class SetAnimatorVariable : Action
{
    public ComponentSelector<Animator> animator;
    public AnimatorControllerParameterType type = AnimatorControllerParameterType.Float;
    public BlackboardEntrySelector<string> parameterName;
    public BlackboardEntrySelector<float> floatValue;
    public BlackboardEntrySelector<int> intValue;
    public BlackboardEntrySelector<bool> boolValue;
    public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
    {
        Animator anim = animator.GetValue(agent);

        Debug.Log(anim);

        if (anim == null)
            return NodeStatus.Failure;

        switch (type)
        {
            case AnimatorControllerParameterType.Int:
                anim.SetInteger(parameterName.value, intValue.value);
                break;
            case AnimatorControllerParameterType.Float:
                anim.SetFloat(parameterName.value, floatValue.value);
                break;
            case AnimatorControllerParameterType.Bool:
                anim.SetBool(parameterName.value, boolValue.value);
                break;
            case AnimatorControllerParameterType.Trigger:
                anim.SetTrigger(parameterName.value);
                break;
        }


        return NodeStatus.Success;
    }
}