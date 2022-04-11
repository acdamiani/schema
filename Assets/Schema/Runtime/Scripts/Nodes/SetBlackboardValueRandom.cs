using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Schema;

[Description("Sets a number to a random value in a given range")]
public class SetBlackboardValueRandom : Action
{
    [DisableDynamicBinding] public BlackboardEntrySelector selector = new BlackboardEntrySelector();
    [SerializeField] private float floatMin;
    [SerializeField] private float floatMax;
    [SerializeField] private int intMin;
    [SerializeField] private int intMax;
    void OnEnable()
    {
        selector.AddFloatFilter();
        selector.AddIntFilter();
    }
    public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
    {
        switch (System.Type.GetTypeCode(selector.entryType))
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
