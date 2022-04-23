using Schema;
using UnityEngine;

public class SetBool : Action
{
    [WriteOnly] public BlackboardEntrySelector<bool> selector;
    public bool value;
    public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
    {
        selector.value = value;

        return NodeStatus.Success;
    }
}