using System.Linq;
using System;
using UnityEngine;
using Schema.Runtime;

[Serializable]
public class BlackboardCondition : Decorator
{
    class BlackboardConditionMemory
    {
        public BlackboardData data;
    }
    public BlackboardEntrySelector blackboardKey = new BlackboardEntrySelector();
    public ConditionType conditionType;
    public enum ConditionType
    {
        IsSet,
        IsNotSet
    }

    [Info]
    private string aborts => "Aborts " + abortsType.ToString();
    private void OnEnable()
    {
        blackboardKey.AddAllFilters();
    }
    public override void OnInitialize(object decoratorMemory, SchemaAgent agent)
    {
        BlackboardConditionMemory memory = (BlackboardConditionMemory)decoratorMemory;
        memory.data = agent.GetBlackboardData();
    }
    public override bool Evaluate(object decoratorMemory, SchemaAgent agent)
    {
        BlackboardConditionMemory memory = (BlackboardConditionMemory)decoratorMemory;
        object val = memory.data.GetValue(blackboardKey.entryID);
        bool isSet = val != null;

        bool ret = conditionType == ConditionType.IsSet ? isSet : !isSet;

        return ret;
    }
}