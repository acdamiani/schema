using UnityEngine;
using Schema;

public class GetTime : Action
{
    [WriteOnly] public BlackboardEntrySelector<float> time;
    public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
    {
        time.value = Time.time;

        return NodeStatus.Success;
    }
}
