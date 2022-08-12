using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("d_Transform Icon", true)]
    [LightIcon("Transform Icon", true)]
    [Description("Find the distance between two Vector values")]
    [Category("Vector")]
    public class Distance : Action
    {
        [Tooltip("Vector A")] public BlackboardEntrySelector vectorOne = new();

        [Tooltip("Vector B")] public BlackboardEntrySelector vectorTwo = new();

        [Tooltip("Blackboard variable to store the distance in")] [WriteOnly]
        public BlackboardEntrySelector<float> distance;

        [Tooltip("Whether to get distance squared, which avoids the expensive square root operation")]
        public bool squared;

        protected override void OnObjectEnable()
        {
            vectorOne.ApplyFilters(typeof(Vector2), typeof(Vector3), typeof(Vector4));
            vectorTwo.ApplyFilters(typeof(Vector2), typeof(Vector3), typeof(Vector4));
        }

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            Vector4 diff = (Vector4)vectorOne.value - (Vector4)vectorTwo.value;
            float dist = squared ? diff.sqrMagnitude : diff.magnitude;

            distance.value = dist;

            return NodeStatus.Success;
        }
    }
}