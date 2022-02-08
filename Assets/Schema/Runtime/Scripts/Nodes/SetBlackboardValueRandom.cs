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
        object value = agent.blackboard.GetValue(selector);
        System.Type valueType = value.GetType();

        switch (System.Type.GetTypeCode(valueType))
        {
            case System.TypeCode.Single:
                agent.blackboard.SetValue<float>(selector.entryID, Random.Range(floatMin, floatMax));
                break;
            case System.TypeCode.Int32:
                agent.blackboard.SetValue<int>(selector.entryID, Random.Range(intMin, intMax));
                break;
        }

        return NodeStatus.Success;
    }
}
