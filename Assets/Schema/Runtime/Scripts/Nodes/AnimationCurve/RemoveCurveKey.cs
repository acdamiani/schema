using Schema;
using UnityEngine;

[Description("Removes a key from an animation curve")]
public class RemoveCurveKey : Action
{
    [Tooltip("Animation curve to use for this operation"), WriteOnly] public BlackboardEntrySelector<AnimationCurve> curve;
    [Tooltip("Index of key to remove")] public BlackboardEntrySelector<int> index;
    void OnValidate()
    {
        index.inspectorValue = Mathf.Clamp(index.inspectorValue, 0, System.Int32.MaxValue);
    }
    public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
    {
        curve.value.RemoveKey(index.value);

        return NodeStatus.Success;
    }
}