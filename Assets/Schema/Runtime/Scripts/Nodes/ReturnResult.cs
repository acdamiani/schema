using UnityEngine;
using Schema;

public class ReturnResult : Schema.Action
{
    public NodeStatus returnsStatus;

    public override NodeStatus Tick(object NodeMemory, SchemaAgent agent)
    {
        return returnsStatus;
    }
}