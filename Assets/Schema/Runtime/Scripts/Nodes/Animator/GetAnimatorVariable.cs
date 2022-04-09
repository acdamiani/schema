using Schema.Runtime;
using UnityEngine;

[RequireAgentComponent(typeof(Animator))]
[DarkIcon("Dark/SetAnimatorVariable")]
[LightIcon("Light/SetAnimatorVariable")]
public class GetAnimatorVariable : Action
{
    public GetAnimatorVariableType type;
    public BlackboardEntrySelector<string> parameterName;
    [WriteOnly] public BlackboardEntrySelector<float> floatValue;
    [WriteOnly] public BlackboardEntrySelector<int> intValue;
    [WriteOnly] public BlackboardEntrySelector<bool> boolValue;
    class SetAnimatorVariableMemory
    {
        public Animator animator;
    }
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
    public enum GetAnimatorVariableType
    {
        Float,
        Int,
        Bool
    }
}