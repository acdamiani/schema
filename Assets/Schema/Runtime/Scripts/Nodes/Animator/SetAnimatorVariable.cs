using Schema.Runtime;
using UnityEngine;

[RequireAgentComponent(typeof(Animator))]
[DarkIcon("Dark/SetAnimatorVariable")]
[LightIcon("Light/SetAnimatorVariable")]
public class SetAnimatorVariable : Action
{
    public ComponentSelector<BoxCollider> animator;
    public AnimatorControllerParameterType type = AnimatorControllerParameterType.Float;
    public BlackboardEntrySelector<string> parameterName;
    public BlackboardEntrySelector<float> floatValue;
    public BlackboardEntrySelector<int> intValue;
    public BlackboardEntrySelector<bool> boolValue;
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
            case AnimatorControllerParameterType.Int:
                memory.animator.SetInteger(parameterName.value, intValue.value);
                break;
            case AnimatorControllerParameterType.Float:
                memory.animator.SetFloat(parameterName.value, floatValue.value);
                break;
            case AnimatorControllerParameterType.Bool:
                memory.animator.SetBool(parameterName.value, boolValue.value);
                break;
            case AnimatorControllerParameterType.Trigger:
                memory.animator.SetTrigger(parameterName.value);
                break;
        }


        return NodeStatus.Success;
    }
}