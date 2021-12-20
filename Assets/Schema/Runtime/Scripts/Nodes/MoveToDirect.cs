using UnityEngine;
using UnityEngine.AI;
using Schema.Runtime;

[Category("Movement")]
[DarkIcon("Dark/MoveToDirect")]
[LightIcon("Light/MoveToDirect")]
internal class MoveToDirect : Action
{
    class MoveToDirectMemory
    {
        public Vector3 point;
        public object lastValue;
    }
    public float speed = 1;
    public bool rotateTowardsTarget;
    public BlackboardEntrySelector selector = new BlackboardEntrySelector();
    private void OnEnable()
    {
        selector.AddObjectFilter();
        selector.AddVector2Filter();
        selector.AddVector3Filter();
    }
    private Vector3 GetPoint(BlackboardEntrySelector selector, BlackboardData data)
    {
        System.Type t = System.Type.GetType(selector.entry.type);
        object value = data.GetValue(selector.entry.Name);

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
    public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
    {
        if (selector.entry != null)
        {
            MoveToDirectMemory memory = (MoveToDirectMemory)nodeMemory;
            memory.point = GetPoint(selector, agent.GetBlackboardData());

            if (rotateTowardsTarget)
            {
                Vector3 target = (agent.transform.position - memory.point).normalized;
                Quaternion rotation = agent.transform.rotation * Quaternion.LookRotation(target);
                agent.transform.rotation = rotation;
            }

            if (Vector3.SqrMagnitude(agent.transform.position - memory.point) < 0.1f)
            {
                return NodeStatus.Success;
            }
            agent.transform.position = Vector3.MoveTowards(agent.transform.position, memory.point, speed * Time.deltaTime);
            return NodeStatus.Running;
        }
        else
        {
            return NodeStatus.Failure;
        }
    }
}