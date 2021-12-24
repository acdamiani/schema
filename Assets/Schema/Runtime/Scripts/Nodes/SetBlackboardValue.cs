using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Schema.Runtime;

public class SetBlackboardValue : Action
{
    class SetBlackboardValueMemory
    {
        public BlackboardData data;
    }
    public BlackboardEntrySelector selector = new BlackboardEntrySelector();
    [SerializeField] private Vector4 vector4Value;
    [SerializeField] private Vector3 vector3Value;
    [SerializeField] private Vector2 vector2Value;
    [SerializeField] private int intValue;
    [SerializeField] private string stringValue;
    [SerializeField] private float floatValue;
    private void OnEnable()
    {
        selector.AddNumericFilter();
        selector.AddStringFilter();
        selector.AddVector2Filter();
        selector.AddVector3Filter();
    }
    public override void OnInitialize(object nodeMemory, SchemaAgent agent)
    {
        SetBlackboardValueMemory memory = (SetBlackboardValueMemory)nodeMemory;

        memory.data = agent.GetBlackboardData();
    }
    public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
    {
        SetBlackboardValueMemory memory = (SetBlackboardValueMemory)nodeMemory;
        object value = memory.data.GetValue(selector);
        System.Type valueType = value.GetType();

        switch (System.Type.GetTypeCode(valueType))
        {
            case System.TypeCode.Single:
                memory.data.SetValue<float>(selector.entryID, floatValue);
                break;
            case System.TypeCode.Int32:
                memory.data.SetValue<int>(selector.entryID, intValue);
                break;
            case System.TypeCode.String:
                memory.data.SetValue<string>(selector.entryID, stringValue);
                break;
            default:
                if (valueType == typeof(Vector4))
                    memory.data.SetValue<Vector4>(selector.entryID, vector4Value);
                else if (valueType == typeof(Vector3))
                    memory.data.SetValue<Vector3>(selector.entryID, vector3Value);
                else if (valueType == typeof(Vector2))
                    memory.data.SetValue<Vector2>(selector.entryID, vector2Value);
                break;
        }

        return NodeStatus.Success;
    }
}
