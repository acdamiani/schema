using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("d_Transform Icon", true), LightIcon("Transform Icon", true), Category("Transform"),
     Description("Moves a transform in the direction and distance of a translation, relative to another Transform")]
    public class TranslateRelative : Action
    {
        [Tooltip("Transform to operate on")] public ComponentSelector<Transform> transform;
        [Tooltip("Translation vector")] public BlackboardEntrySelector<Vector3> translation;

        [Tooltip("Transform to translate relative to")]
        public ComponentSelector<Transform> relativeTo;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            Transform t1 = agent.GetComponent(transform);

            if (t1 == null)
                return NodeStatus.Failure;

            Transform t2 = agent.GetComponent(relativeTo);

            t1.Translate(translation.value, t2);

            return NodeStatus.Success;
        }
    }
}