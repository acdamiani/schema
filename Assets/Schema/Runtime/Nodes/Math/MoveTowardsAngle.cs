using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("Nodes/d_Math"), LightIcon("Nodes/Math"), Category("Math"),
     Description("Move an angle towards another angle by a specified step size, wrapping around 360 degrees")]
    public class MoveTowardsAngle : Action
    {
        [Tooltip("Current angle")] public BlackboardEntrySelector<float> current;

        [Tooltip("Whether the current angle is in radians")]
        public bool currentIsRadians;

        [Tooltip("Target angle")] public BlackboardEntrySelector<float> target;

        [Tooltip("Whether the target angle is in radians")]
        public bool targetIsRadians;

        [Tooltip("Max delta to move towards the target")]
        public BlackboardEntrySelector<float> maxDelta;

        [Tooltip("The lerped angle"), WriteOnly] 
        public BlackboardEntrySelector<float> result;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            float a1 = currentIsRadians ? current.value * Mathf.Rad2Deg : current.value;
            float a2 = targetIsRadians ? target.value * Mathf.Rad2Deg : target.value;

            result.value = Mathf.MoveTowardsAngle(a1, a2, maxDelta.value);

            return NodeStatus.Success;
        }
    }
}