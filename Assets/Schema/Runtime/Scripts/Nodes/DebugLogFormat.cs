using Schema;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[DarkIcon("Dark/DebugLog")]
[LightIcon("Light/DebugLog")]
[Description("Logs a formatted message to the console.")]
public class DebugLogFormat : Action
{
    [TextArea] public string message;
    public List<BlackboardEntrySelector> keys;
    protected override void OnNodeEnable()
    {
        if (keys != null)
        {
            foreach (BlackboardEntrySelector key in keys)
                key.AddAllFilters();
        }
    }
    private void OnValidate()
    {
        if (keys != null)
        {
            foreach (BlackboardEntrySelector key in keys)
                key.AddAllFilters();
        }
    }
    public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
    {
        object[] values = keys.Select(key => key.value).ToArray();

        try
        {
            Debug.LogFormat(message, values);
            return NodeStatus.Success;
        }
        catch (System.Exception e)
        {
            Debug.LogException(e);
            return NodeStatus.Failure;
        }
    }
}