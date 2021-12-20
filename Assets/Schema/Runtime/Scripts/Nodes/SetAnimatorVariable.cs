using Schema.Runtime;
using UnityEngine;

[RequireAgentComponent(typeof(Animator))]
[DarkIcon("Dark/SetAnimatorVariable")]
[LightIcon("Light/SetAnimatorVariable")]
public class SetAnimatorVariable : Action
{
    public AnimatorVariableValue variableValue;
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

        switch (variableValue.variableType)
        {
            case AnimatorVariableValue.VariableType.Integer:
                memory.animator.SetInteger(variableValue.variableName, variableValue.intValue);
                break;
            case AnimatorVariableValue.VariableType.Float:
                memory.animator.SetFloat(variableValue.variableName, variableValue.floatValue);
                break;
            case AnimatorVariableValue.VariableType.Bool:
                memory.animator.SetBool(variableValue.variableName, variableValue.boolValue);
                break;
        }

        return NodeStatus.Success;
    }
}
[System.Serializable]
public class AnimatorVariableValue
{
    public enum VariableType
    {
        Integer,
        Float,
        Bool
    }
    public VariableType variableType;
    public string variableName;
    public int intValue;
    public float floatValue;
    public bool boolValue;
}