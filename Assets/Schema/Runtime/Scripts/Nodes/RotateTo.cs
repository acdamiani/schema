using Schema;
using UnityEngine;

public class RotateTo : Schema.Action
{
    public BlackboardEntrySelector<Vector3> selector;
    [Tooltip("Speed of rotation, in deg/sec")]
    public float speed;
    [Tooltip("Enable smooth interpolation")]
    public bool slerp = true;
    class RotateToMemory
    {
        public Vector3 forwardInitial;
        public Quaternion rotationInitial;
        public float t;
    }
    public override void OnNodeEnter(object nodeMemory, SchemaAgent agent)
    {
        RotateToMemory memory = (RotateToMemory)nodeMemory;

        memory.forwardInitial = agent.transform.forward;
        memory.rotationInitial = agent.transform.rotation;
        memory.t = Time.time;
    }
    public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
    {
        RotateToMemory memory = (RotateToMemory)nodeMemory;

        Vector3 point = selector.value;

        Debug.Log(point);

        Quaternion rotation = Quaternion.FromToRotation(memory.forwardInitial, (point - agent.transform.position).normalized);
        float angleDiff = Vector3.Angle(agent.transform.forward, (point - agent.transform.position).normalized);

        if (slerp)
            agent.transform.rotation = Quaternion.Slerp(agent.transform.rotation, memory.rotationInitial * rotation, Time.deltaTime * speed / angleDiff);
        else
            agent.transform.rotation = Quaternion.Lerp(agent.transform.rotation, rotation, Time.deltaTime * speed / angleDiff);

        if (Mathf.Abs(angleDiff) > 0.0001f)
            return NodeStatus.Running;
        else
            return NodeStatus.Success;
    }
}