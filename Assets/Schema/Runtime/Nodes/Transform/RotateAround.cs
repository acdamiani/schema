using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("d_Transform Icon", true), LightIcon("Transform Icon", true), Category("Transform"), Description(
         "Rotates a transform about an axis passing through a point point in world coordinates by a degree measure")]
    public class RotateAround : Action
    {
        [Tooltip("Transform to operate on")] public ComponentSelector<Transform> transform;
        [Tooltip("Target world position")] public BlackboardEntrySelector<Vector3> target;

        [Tooltip("The axis of rotation (defaults to Vector3.up)")]
        public BlackboardEntrySelector<Vector3> axis = new BlackboardEntrySelector<Vector3>(Vector3.up);

        [Tooltip("Number of degrees to rotate")]
        public BlackboardEntrySelector<float> angle;

        protected override void OnObjectEnable()
        {
            axis.inspectorValue.Normalize();
        }

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            Transform t = agent.GetComponent(transform);

            if (t == null)
                return NodeStatus.Failure;

            Vector3 a = axis.value.normalized;
            t.RotateAround(target.value, a, angle.value);

            return NodeStatus.Success;
        }
    }
}