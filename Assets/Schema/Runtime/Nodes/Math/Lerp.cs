using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("Nodes/d_Math"), LightIcon("Nodes/Math"), Category("Math"),
     Description("Linearly interpolate between two floats by a time parameter")]
    public class Lerp : Action
    {
        [Tooltip("Current float")] public BlackboardEntrySelector<float> current;
        [Tooltip("Target float")] public BlackboardEntrySelector<float> target;
        [Tooltip("Parameter t to lerp by")] public BlackboardEntrySelector<float> t;
        [Tooltip("Do not limit t")] public bool unclamped;

        [Tooltip("The lerped float"), WriteOnly] 
        public BlackboardEntrySelector<float> result;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            if (unclamped)
                result.value = Mathf.LerpUnclamped(current.value, target.value, t.value);
            else
                result.value = Mathf.Lerp(current.value, target.value, t.value);

            return NodeStatus.Success;
        }
    }
}