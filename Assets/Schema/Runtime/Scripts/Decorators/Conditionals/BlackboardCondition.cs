using System.Linq;
using System;
using UnityEngine;
using Schema.Runtime;

[Serializable]
public class BlackboardCondition : Decorator
{
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
    public override bool Evaluate(object decoratorMemory, SchemaAgent agent)
    {
        object val = agent.blackboard.GetValue(blackboardKey.entryID);
        bool isSet = val != null;

        bool ret = conditionType == ConditionType.IsSet ? isSet : !isSet;

        return ret;
    }
}