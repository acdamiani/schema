using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("d_Transform Icon", true), LightIcon("Transform Icon", true), Category("Transform"),
     Description("Rotate a transform by applying a Euler Quaternion")]
    public class Rotate : Action
    {
        [Tooltip("Transform to operate on")] public ComponentSelector<Transform> transform;

        [Tooltip("New rotation in euler angles")]
        public BlackboardEntrySelector<Vector3> eulerAngles;

        [Tooltip(
            "Determines whether to rotate the GameObject either locally to the GameObject or relative to the Scene in world space.")]
        public Space relativeTo;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            Transform t = agent.GetComponent(transform);

            if (transform.value == null)
                return NodeStatus.Failure;

            t.Rotate(eulerAngles.value, relativeTo);

            return NodeStatus.Success;
        }
    }
}