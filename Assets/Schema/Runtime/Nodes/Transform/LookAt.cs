using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("d_Transform Icon", true), LightIcon("Transform Icon", true), Category("Transform"),
     Description("Rotates a transform so the forward vector points at a target's current position")]
    public class LookAt : Action
    {
        [Tooltip("Transform to operate on")] public ComponentSelector<Transform> transform;
        [Tooltip("Target transform")] public ComponentSelector<Transform> target;

        [Tooltip("Up direction (defaults to Vector3.up)")]
        public BlackboardEntrySelector<Vector3> worldUp = new BlackboardEntrySelector<Vector3>(Vector3.up);

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            Transform t1 = agent.GetComponent(transform);
            Transform t2 = agent.GetComponent(target);

            if (t1 == null || t2 == null)
                return NodeStatus.Failure;

            t1.LookAt(t2, worldUp.value);

            return NodeStatus.Success;
        }
    }
}