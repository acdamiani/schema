using Schema;
using UnityEngine;

public class RotateAway : Schema.Action
{
    public BlackboardEntrySelector selector = new BlackboardEntrySelector();
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
    void OnEnable()
    {
        selector.AddVector2Filter();
        selector.AddVector3Filter();
        selector.AddGameObjectFilter();
    }
    private Vector3 GetPoint(BlackboardEntrySelector selector)
    {
        object value = selector.value;
        System.Type t = value.GetType();

        if (value == null) return Vector3.zero;

        //Not ideal to run every frame, so will be cached in the node state	
        if (t == typeof(GameObject))
        {
            return ((GameObject)value).transform.position;
        }
        else if (t == typeof(Vector2))
        {
            return (Vector2)value;
        }
        else if (t == typeof(Vector2Int))
        {
            return (Vector2)value;
        }
        else if (t == typeof(Vector3))
        {
            return (Vector3)value;
        }
        else if (t == typeof(Vector3Int))
        {
            return (Vector3)value;
        }
        else
        {
            return Vector3.zero;
        }
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

        Vector3 point = GetPoint(selector);
        Quaternion rotation = Quaternion.FromToRotation(memory.forwardInitial, (agent.transform.position - point).normalized);
        float angleDiff = Vector3.Angle(agent.transform.forward, (agent.transform.position - point).normalized);

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
