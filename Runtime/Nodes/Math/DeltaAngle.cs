using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("Nodes/d_Math"), LightIcon("Nodes/Math"), Category("Math"),
     Description("Gets the shortest angular distance between two angles")]
    public class DeltaAngle : Action
    {
        [Tooltip("Current angular position")] public BlackboardEntrySelector<float> current;
        [Tooltip("Target angular position")] public BlackboardEntrySelector<float> target;

        [Tooltip("Whether current angular position is in radians")]
        public bool currentIsRadians;

        [Tooltip("Whether target angular position is in radians")]
        public bool targetIsRadians;

        [Tooltip("Shortest angular distance between the two values"), WriteOnly] 
        public BlackboardEntrySelector<float> delta;

        [Tooltip("Store the delta as radians instead of degrees")]
        public bool storeRadians;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            float a1 = currentIsRadians ? current.value * Mathf.Rad2Deg : current.value;
            float a2 = targetIsRadians ? target.value * Mathf.Rad2Deg : target.value;
            float diff = Mathf.DeltaAngle(a1, a2);

            delta.value = storeRadians ? diff * Mathf.Deg2Rad : diff;

            return NodeStatus.Success;
        }
    }
}