using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [Description("Gets slope of line tangent to an Animation Curve at t"), DarkIcon("Nodes/d_AnimationCurve"),
     LightIcon("Nodes/AnimationCurve"), Category("Animation")]
    public class Slope : Action
    {
        private const double h = 1e-7f;

        [Tooltip("Animation curve to use for this operation")]
        public BlackboardEntrySelector<AnimationCurve> curve;

        [Tooltip("t value for this graph (horizontal axis)")]
        public BlackboardEntrySelector<float> t;

        [Tooltip("Entry to store the result of this operation"), WriteOnly] 
        public BlackboardEntrySelector<float> result;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            result.value = GetTangent(curve.value, t.value);

            return NodeStatus.Success;
        }

        private float GetTangent(AnimationCurve curve, float t)
        {
            double a = curve.Evaluate((float)(t + h));
            double b = curve.Evaluate((float)(t - h));
            double c = 2 * h;
            double d = a - b;

            return (float)(d / c);
        }
    }
}