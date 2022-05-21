using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("d_math")]
    [LightIcon("math")]
    [Category("Math")]
    [Description("Generate perlin noise from a Vector2 location")]
    public class PerlinNoise : Action
    {
        [Tooltip("Location of sample point")] public BlackboardEntrySelector<Vector2> location;
        [Tooltip("Sampled perlin value"), WriteOnly] public BlackboardEntrySelector<float> result;
        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            result.value = Mathf.PerlinNoise(location.value.x, location.value.y);

            return NodeStatus.Success;
        }
    }
}