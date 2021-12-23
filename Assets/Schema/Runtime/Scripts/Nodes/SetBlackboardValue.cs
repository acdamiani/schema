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

        return NodeStatus.Success;
    }
}
