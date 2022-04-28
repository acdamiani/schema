using UnityEngine;
using Schema;

public class GetVectorComponents : Action
{
    [Tooltip("Vector to use to get components")] public BlackboardEntrySelector vector;
    [WriteOnly] public BlackboardEntrySelector<float> x;
    [WriteOnly] public BlackboardEntrySelector<float> y;
    [WriteOnly] public BlackboardEntrySelector<float> z;
    [WriteOnly] public BlackboardEntrySelector<float> w;
    protected override void OnNodeEnable()
    {
        vector.AddVector2Filter();
        vector.AddVector3Filter();
        vector.AddVector4Filter();
    }
    public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
    {
        return NodeStatus.Success;
    }
}