using Schema.Runtime;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[DarkIcon("Dark/DebugLog")]
[LightIcon("Light/DebugLog")]
[Description("Logs a rich-text enabled message to the console")]
public class DebugLog : Action
{
    [TextArea] public string message;
    public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
    {
        Debug.Log(message);

        return NodeStatus.Success;
    }
}