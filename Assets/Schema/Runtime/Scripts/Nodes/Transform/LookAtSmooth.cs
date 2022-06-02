using Schema;
using UnityEngine;

[DarkIcon("d_Transform Icon", true)]
[LightIcon("Transform Icon", true)]
[Category("Transform")]
[Description("Rotate a Transform towards a target smoothly")]
public class LookAtSmooth : Schema.Action
{
    [Tooltip("Transform to operate on")] public ComponentSelector<Transform> transform;
    [Tooltip("Target to rotate towards")] public ComponentSelector<Transform> target;
    [Tooltip("Speed of rotation, in deg/sec")]
    public float speed;
    [Tooltip("Enable smooth interpolation")]
    public bool slerp = true;
    class RotateToMemory
    {
        public float t;
    }
    public override void OnNodeEnter(object nodeMemory, SchemaAgent agent)
    {
        RotateToMemory memory = (RotateToMemory)nodeMemory;

        memory.t = Time.time;
    }
    public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
    {
        Transform self = agent.GetComponent(transform);
        Transform targetTransform = agent.GetComponent(target);

        RotateToMemory memory = (RotateToMemory)nodeMemory;

        Quaternion rotation = Quaternion.FromToRotation(self.forward, (targetTransform.position - agent.transform.position).normalized);
        float angleDiff = Vector3.Angle(agent.transform.forward, (targetTransform.position - agent.transform.position).normalized);

        if (slerp)
            agent.transform.rotation = Quaternion.Slerp(agent.transform.rotation, self.rotation * rotation, Time.deltaTime * speed / angleDiff);
        else
            agent.transform.rotation = Quaternion.Lerp(agent.transform.rotation, rotation, Time.deltaTime * speed / angleDiff);

        if (Mathf.Abs(angleDiff) > 0.0001f)
            return NodeStatus.Running;
        else
            return NodeStatus.Success;
    }
}