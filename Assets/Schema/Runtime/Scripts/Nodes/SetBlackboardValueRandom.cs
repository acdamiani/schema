using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Schema.Runtime;

public class SetBlackboardValueRandom : Action
{
    public BlackboardEntrySelector selector = new BlackboardEntrySelector(typeof(float), typeof(int));
    [SerializeField] private float floatMin;
    [SerializeField] private float floatMax;
    [SerializeField] private int intMin;
    [SerializeField] private int intMax;
    public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
    {
        object value = selector.value;
        System.Type valueType = value.GetType();

        switch (System.Type.GetTypeCode(valueType))
        {
            case System.TypeCode.Single:
                selector.value = Random.Range(floatMin, floatMax);
                break;
            case System.TypeCode.Int32:
                selector.value = Random.Range(intMin, intMax);
                break;
        }

        return NodeStatus.Success;
    }
}
