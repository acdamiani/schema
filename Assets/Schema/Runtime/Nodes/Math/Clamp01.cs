using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("Nodes/d_Math"), LightIcon("Nodes/Math"), Category("Math"),
     Description("Clamp a float to be between 0 and 1")]
    public class Clamp01 : Action
    {
        [Tooltip("Float to clamp")] public BlackboardEntrySelector<float> value;
        [Tooltip("Clamped float"), WriteOnly]  public BlackboardEntrySelector<float> result;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            result.value = Mathf.Clamp01(value.value);

            return NodeStatus.Success;
        }
    }
}