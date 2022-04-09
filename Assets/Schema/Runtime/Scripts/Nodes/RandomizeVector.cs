using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Schema.Runtime;

[Description("Randomize the values of a vector within given ranges")]
public class RandomizeVector : Action
{
    [DisableDynamicBinding] public BlackboardEntrySelector entrySelector = new BlackboardEntrySelector();
    public Vector2[] minMax;
    void OnEnable()
    {
        entrySelector.AddVector2Filter();
        entrySelector.AddVector3Filter();
        entrySelector.AddVector4Filter();
    }
    public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
    {
        return NodeStatus.Success;
    }
}
