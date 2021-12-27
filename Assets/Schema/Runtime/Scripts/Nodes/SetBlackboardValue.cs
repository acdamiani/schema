using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Schema.Runtime;

public class SetBlackboardValue : Action
{
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
    public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
    {
        BlackboardData blackboard = agent.blackboard;

        object value = blackboard.GetValue(selector);
        System.Type valueType = value.GetType();

        switch (System.Type.GetTypeCode(valueType))
        {
            case System.TypeCode.Single:
                blackboard.SetValue<float>(selector.entryID, floatValue);
                break;
            case System.TypeCode.Int32:
                blackboard.SetValue<int>(selector.entryID, intValue);
                break;
            case System.TypeCode.String:
                blackboard.SetValue<string>(selector.entryID, stringValue);
                break;
            default:
                if (valueType == typeof(Vector4))
                    blackboard.SetValue<Vector4>(selector.entryID, vector4Value);
                else if (valueType == typeof(Vector3))
                    blackboard.SetValue<Vector3>(selector.entryID, vector3Value);
                else if (valueType == typeof(Vector2))
                    blackboard.SetValue<Vector2>(selector.entryID, vector2Value);
                break;
        }

        return NodeStatus.Success;
    }
}
