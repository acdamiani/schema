using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("Nodes/d_AnimationCurve"), LightIcon("Nodes/AnimationCurve"),
     Description("Evaluate an animation curve at a time"), Category("Animation")]
    public class SampleValue : Action
    {
        [Tooltip("Curve to evaluate")] public BlackboardEntrySelector<AnimationCurve> curve;

        [Tooltip("t value to sample at (horizontal axis)")]
        public BlackboardEntrySelector<float> tValue;

        [Tooltip("Where to store result of this operation"), WriteOnly] 
        public BlackboardEntrySelector<float> target;

        private void OnValidate()
        {
            tValue.inspectorValue = Mathf.Clamp01(tValue.inspectorValue);
        }

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            float t = tValue.value % 1f;

            target.value = curve.value.Evaluate(t);

            return NodeStatus.Success;
        }
    }
}