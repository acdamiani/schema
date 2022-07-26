using Schema;
using System;
using System.Reflection;
using UnityEngine;

public class IsNull : Conditional
{
    [Tooltip("Entry to check for null")] public BlackboardEntrySelector entry;
    class IsNullMemory
    {
        public object defaultValue;
    }
    protected override void OnObjectEnable()
    {
        entry.ApplyAllFilters();
    }
    void OnValidate()
    {
        Debug.Log(entry.entryType);
    }
    public override void OnInitialize(object decoratorMemory, SchemaAgent agent)
    {
        IsNullMemory memory = (IsNullMemory)decoratorMemory;

        Type entryType = entry.entryType;

        if (entryType.IsValueType)
            memory.defaultValue = Activator.CreateInstance(entryType);
        else
            memory.defaultValue = null;
    }
    public override bool Evaluate(object decoratorMemory, SchemaAgent agent)
    {
        IsNullMemory memory = (IsNullMemory)decoratorMemory;

        Debug.Log(memory.defaultValue);

        return entry.value != null;
    }
}