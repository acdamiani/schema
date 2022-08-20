using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("d_Transform Icon", true), LightIcon("Transform Icon", true), Category("Transform"),
     Description("Transform a given position from local to world space")]
    public class TransformPoint : Action
    {
        [Tooltip("Transform to operate on")] public ComponentSelector<Transform> transform;
        [Tooltip("Point vector")] public BlackboardEntrySelector<Vector3> point;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            Transform t = agent.GetComponent(transform);

            if (t == null)
                return NodeStatus.Failure;

            t.TransformPoint(point.value);

            return NodeStatus.Success;
        }
    }
}