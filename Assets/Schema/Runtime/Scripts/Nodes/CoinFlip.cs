using UnityEngine;
using Schema.Runtime;

public class CoinFlip : Action
{
    [WriteOnly] public BlackboardEntrySelector<bool> entry;
    [Tooltip("Chance that the entry will be true")][Range(0, 1)] public float chance;
    public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
    {

        bool v = Random.Range(0f, 1f) <= chance;
        Debug.Log(v);
        entry.value = v;

        return NodeStatus.Success;
    }
}