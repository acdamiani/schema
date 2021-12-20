using UnityEngine;
using UnityEngine.AI;
using Schema.Runtime;

[Category("Movement")]
[DarkIcon("Dark/MoveTo")]
[LightIcon("Light/MoveTo")]
[RequireAgentComponent(typeof(NavMeshAgent))]
public class MoveTo : Action
{
    class MoveToMemory
    {
        public NavMeshAgent agent;
        public Vector3 point;
        public object lastValue;
    }
    public float speed = 1;
    public float acceptableRadius = 1f;
    [Tooltip("How fast the agent can turn to look at a target, given in deg/sec")]
    public float angularSpeed = 120f;
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
        MoveToMemory memory = (MoveToMemory)nodeMemory;
        memory.agent = agent.GetComponent<NavMeshAgent>();
    }
    public override void OnNodeExit(object nodeMemory, SchemaAgent agent)
    {
        MoveToMemory memory = (MoveToMemory)nodeMemory;
        memory.agent.ResetPath();
    }
    public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
    {
        if (selector.entry != null)
        {
            MoveToMemory memory = (MoveToMemory)nodeMemory;
            memory.point = GetPoint(selector, agent.GetBlackboardData());
            memory.agent.speed = speed;
            memory.agent.stoppingDistance = acceptableRadius;
            memory.agent.angularSpeed = angularSpeed;

            if (memory.agent.SetDestination(memory.point))
            {
                if (!memory.agent.pathPending &&
                (memory.agent.remainingDistance <= memory.agent.stoppingDistance) &&
                (!memory.agent.hasPath || memory.agent.velocity.sqrMagnitude == 0f))
                {
                    return NodeStatus.Success;
                }

                return NodeStatus.Running;
            }
            else
            {
                return NodeStatus.Failure;
            }
        }
        else
        {
            return NodeStatus.Failure;
        }
    }
}