using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("d_Transform Icon", true), LightIcon("Transform Icon", true), Category("Transform"),
     Description("Moves a transform in the direction and distance of a translation")]
    public class Translate : Action
    {
        [Tooltip("Transform to operate on")] public ComponentSelector<Transform> transform;
        [Tooltip("Translation vector")] public BlackboardEntrySelector<Vector3> translation;

        [Tooltip(
            "Space to translate relative to (to Translate relative to another Transform, use the TranslateRelative node)")]
        public Space relativeTo;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            Transform t = agent.GetComponent(transform);

            if (t == null)
                return NodeStatus.Failure;

            t.Translate(translation.value, relativeTo);

            return NodeStatus.Success;
        }
    }
}