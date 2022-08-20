using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("Nodes/d_Math"), LightIcon("Nodes/Math"), Category("Math"),
     Description("Linearly interpolate between two floats by a time parameter, wrapping properly around 360 degrees")]
    public class LerpAngle : Action
    {
        [Tooltip("Current angle")] public BlackboardEntrySelector<float> current;

        [Tooltip("Whether the current angle is measured in radians")]
        public bool currentIsRadians;

        [Tooltip("Target angle")] public BlackboardEntrySelector<float> target;

        [Tooltip("Whether the target angle is measured in radians")]
        public bool targetIsRadians;

        [Tooltip("Parameter t to lerp by")] public BlackboardEntrySelector<float> t;

        [Tooltip("The lerped angle"), WriteOnly] 
        public BlackboardEntrySelector<float> result;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            float a1 = currentIsRadians ? current.value * Mathf.Rad2Deg : current.value;
            float a2 = targetIsRadians ? target.value * Mathf.Rad2Deg : target.value;

            result.value = Mathf.LerpAngle(a1, a2, t.value);

            return NodeStatus.Success;
        }
    }
}