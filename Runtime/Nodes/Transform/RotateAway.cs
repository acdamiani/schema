using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("d_Transform Icon", true), LightIcon("Transform Icon", true), Category("Transform")]
    public class RotateAway : Action
    {
        public ComponentSelector<Transform> transform;
        public BlackboardEntrySelector<Vector3> point;

        [Tooltip("Speed of rotation, in deg/sec")]
        public float speed;

        [Tooltip("Enable smooth interpolation")]
        public bool slerp = true;

        public override void OnNodeEnter(object nodeMemory, SchemaAgent agent)
        {
            RotateToMemory memory = (RotateToMemory)nodeMemory;

            Transform t = agent.GetComponent(transform);

            if (t == null)
                return;

            memory.forwardInitial = t.forward;
            memory.rotationInitial = t.rotation;
            memory.t = Time.time;
        }

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            RotateToMemory memory = (RotateToMemory)nodeMemory;

            Transform t = agent.GetComponent(transform);

            if (t == null)
                return NodeStatus.Failure;

            Quaternion rotation =
                Quaternion.FromToRotation(memory.forwardInitial, (t.position - point.value).normalized);
            float angleDiff = Vector3.Angle(t.forward, (agent.transform.position - point.value).normalized);

            if (slerp)
                t.rotation = Quaternion.Slerp(t.rotation, memory.rotationInitial * rotation,
                    Time.deltaTime * speed / angleDiff);
            else
                t.rotation =
                    Quaternion.Lerp(t.rotation, rotation, Time.deltaTime * speed / angleDiff);

            if (Mathf.Abs(angleDiff) > 0.0001f)
                return NodeStatus.Running;
            return NodeStatus.Success;
        }

        private class RotateToMemory
        {
            public Vector3 forwardInitial;
            public Quaternion rotationInitial;
            public float t;
        }
    }
}