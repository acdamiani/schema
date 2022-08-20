using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("Nodes/d_Math"), LightIcon("Nodes/Math"), Category("Math"),
     Description("Interpolate between two floats by a time parameter, with smoothing at the limits")]
    public class SmoothStep : Action
    {
        [Tooltip("Current float")] public BlackboardEntrySelector<float> current;
        [Tooltip("Target float")] public BlackboardEntrySelector<float> target;

        [Tooltip("Parameter t to interpolate by")]
        public BlackboardEntrySelector<float> t;

        [Tooltip("The interpolated float"), WriteOnly] 
        public BlackboardEntrySelector<float> result;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            result.value = Mathf.SmoothStep(current.value, target.value, t.value);

            return NodeStatus.Success;
        }
    }
}