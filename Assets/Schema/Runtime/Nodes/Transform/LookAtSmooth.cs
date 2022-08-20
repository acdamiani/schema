using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("d_Transform Icon", true), LightIcon("Transform Icon", true), Category("Transform"),
     Description("Rotate a Transform towards a target smoothly")]
    public class LookAtSmooth : Action
    {
        [Tooltip("Transform to operate on")] public ComponentSelector<Transform> transform;
        [Tooltip("Target to rotate towards")] public ComponentSelector<Transform> target;

        [Tooltip("Speed of rotation, in deg/sec")]
        public float speed;

        [Tooltip("Enable smooth interpolation")]
        public bool slerp = true;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            Transform self = agent.GetComponent(transform);
            Transform targetTransform = agent.GetComponent(target);

            Vector3 dir = Vector3.Normalize(targetTransform.position - self.position);

            Quaternion rotation = Quaternion.LookRotation(dir);
            float angleDiff = Vector3.Angle(self.forward, dir);

            if (slerp)
                self.rotation = Quaternion.Slerp(self.rotation, rotation, Time.deltaTime * speed / angleDiff);
            else
                self.rotation = Quaternion.Lerp(self.rotation, rotation, Time.deltaTime * speed / angleDiff);

            if (Mathf.Abs(angleDiff) > 0.0001f)
                return NodeStatus.Running;
            return NodeStatus.Success;
        }
    }
}