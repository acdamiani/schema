using UnityEngine;
using Schema.Runtime;

public class ReturnResult : Schema.Runtime.Action
{
    public NodeStatus returnsStatus;

    public override NodeStatus Tick(object NodeMemory, SchemaAgent agent)
    {
        return returnsStatus;
    }
}