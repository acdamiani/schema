using System.Linq;
using System;
using UnityEngine;
using Schema;

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

    private string aborts => "Aborts " + abortsType.ToString();
    private void OnEnable()
    {
        blackboardKey.ApplyAllFilters();
    }
    public override bool Evaluate(object decoratorMemory, SchemaAgent agent)
    {
        object val = blackboardKey.value;
        bool isSet = val != null;

        bool ret = conditionType == ConditionType.IsSet ? isSet : !isSet;

        return ret;
    }
}