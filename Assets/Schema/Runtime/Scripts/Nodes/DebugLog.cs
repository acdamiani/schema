using Schema.Runtime;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[DarkIcon("Dark/DebugLog")]
[LightIcon("Light/DebugLog")]
public class DebugLog : Action
{
    [TextArea] public string message;
    public List<BlackboardEntrySelector> keys;
    class DebugLogMemory
    {
        public BlackboardData data;
    }
    private void OnEnable()
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
    public override void OnInitialize(object nodeMemory, SchemaAgent agent)
    {
        DebugLogMemory memory = (DebugLogMemory)nodeMemory;

        memory.data = agent.GetBlackboardData();
    }
    public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
    {
        DebugLogMemory memory = (DebugLogMemory)nodeMemory;

        object[] values = keys.Select(key => memory.data.GetValue(key.entryID)).ToArray();

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