using Schema.Runtime;
using UnityEngine;

[DarkIcon("Dark/DebugLog")]
[LightIcon("Light/DebugLog")]
public class DebugLog : Action
{
    public string message;
    public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
    {
        Debug.Log(message);

        return NodeStatus.Success;
    }
}