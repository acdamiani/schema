using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Schema.Runtime;

public class SetBlackboardValueRandom : Action
{
    class SetBlackboardValueRandomMemory
    {
        public BlackboardData data;
    }
    public BlackboardNumber selector;
    [SerializeField] private float floatMin;
    [SerializeField] private float floatMax;
    [SerializeField] private int intMin;
    [SerializeField] private int intMax;
    public override void OnInitialize(object nodeMemory, SchemaAgent agent)
    {
        SetBlackboardValueRandomMemory memory = (SetBlackboardValueRandomMemory)nodeMemory;

        memory.data = agent.GetBlackboardData();
    }

    public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
    {
        SetBlackboardValueRandomMemory memory = (SetBlackboardValueRandomMemory)nodeMemory;
        object value = memory.data.GetValue(selector);
        System.Type valueType = value.GetType();

        switch (System.Type.GetTypeCode(valueType))
        {
            case System.TypeCode.Single:
                memory.data.SetValue<float>(selector.entryID, Random.Range(floatMin, floatMax));
                break;
            case System.TypeCode.Int32:
                memory.data.SetValue<int>(selector.entryID, Random.Range(intMin, intMax));
                break;
        }

        return NodeStatus.Success;
    }
}
