using Schema.Runtime;
using UnityEngine;

[Category("Movement")]
public class RotateTo : Schema.Runtime.Action
{
    public BlackboardEntrySelector selector = new BlackboardEntrySelector();
    public bool lockXRotation = true;
    public bool lockYRotation;
    public bool lockZRotation = true;
    class RotateToMemory
    {
        public BlackboardData data;
    }
    void OnEnable()
    {
        selector.AddVector2Filter();
        selector.AddVector3Filter();
        selector.AddObjectFilter();
    }
    private Vector3 GetPoint(BlackboardEntrySelector selector, BlackboardData data)
    {
        System.Type t = System.Type.GetType(selector.entry.type);
        object value = data.GetValue(selector.entry.Name);

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
    public override void OnInitialize(object nodeMemory, SchemaAgent agent)
    {
        RotateToMemory memory = (RotateToMemory)nodeMemory;

        memory.data = agent.GetBlackboardData();
    }
    public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
    {
        RotateToMemory memory = (RotateToMemory)nodeMemory;
        Vector3 point = GetPoint(selector, memory.data);
        Quaternion rotation = agent.transform.rotation * Quaternion.LookRotation(point, Vector3.up);

        agent.transform.rotation = rotation;

        return NodeStatus.Success;
    }
}