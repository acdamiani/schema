using Schema;
using UnityEngine;

[AllowOnlyOne]
internal class ForceSuccess : Decorator
{
    public override bool OnNodeProcessed(object decoratorMemory, SchemaAgent agent, ref NodeStatus status)
    {
        status = NodeStatus.Success;
        return false;
    }
}