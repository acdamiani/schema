using Schema;
using UnityEngine;

[RequireAgentComponent(typeof(Animator))]
[DarkIcon("c_Animator")]
[LightIcon("c_Animator")]
public class GetAnimatorVariable : Action
{
    public enum GetAnimatorVariableType
    {
        Float,
        Int,
        Bool
    }

    public GetAnimatorVariableType type;
    public BlackboardEntrySelector<string> parameterName;
    [WriteOnly] public BlackboardEntrySelector<float> floatValue;
    [WriteOnly] public BlackboardEntrySelector<int> intValue;
    [WriteOnly] public BlackboardEntrySelector<bool> boolValue;

    public override void OnInitialize(object nodeMemory, SchemaAgent agent)
    {
        ((SetAnimatorVariableMemory)nodeMemory).animator = agent.GetComponent<Animator>();
    }

    public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
    {
        SetAnimatorVariableMemory memory = (SetAnimatorVariableMemory)nodeMemory;

        switch (type)
        {
            case GetAnimatorVariableType.Int:
                intValue.value = memory.animator.GetInteger(parameterName.value);
                break;
            case GetAnimatorVariableType.Float:
                floatValue.value = memory.animator.GetFloat(parameterName.value);
                break;
            case GetAnimatorVariableType.Bool:
                boolValue.value = memory.animator.GetBool(parameterName.value);
                break;
        }

        return NodeStatus.Success;
    }

    private class SetAnimatorVariableMemory
    {
        public Animator animator;
    }
}