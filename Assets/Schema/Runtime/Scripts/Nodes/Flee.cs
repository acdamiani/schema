using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Schema.Runtime;

[DarkIcon("Dark/Flee")]
[LightIcon("Light/Flee")]
[RequireAgentComponent(typeof(NavMeshAgent))]
public class Flee : Action
{
    [Tooltip("The enemy GameObject")]
    public BlackboardEntrySelector<GameObject> enemy;
    [Tooltip("The NavMesh surfaces the agent is allowed to choose points on")]
    public NavMeshAreaMask areaMask;
    [Range(10f, 180f)]
    [Tooltip("The maximum angle to choose a point in front of the agent")]
    public float angle = 45f;
    [Min(1f)]
    [Tooltip("The maximum distance to choose a point")]
    public float maxDistance = 10f;
    [Tooltip("The minimum distance to choose a point")]
    public float minDistance = 2f;
    [Tooltip("The distance away from the enemy that the agent considers \"safe.\"")]
    public float safeDistance = 25f;
    [Tooltip("Visualize the range")]
    public bool visualize = false;
    private Vector3 randomPoint;
    class FleeMemory
    {
        public NavMeshAgent agent;
        public GameObject enemy;
        public Vector3 point;
    }
    void OnValidate()
    {
        maxDistance = Mathf.Clamp(maxDistance, 0, float.MaxValue);
        minDistance = Mathf.Clamp(minDistance, 0, maxDistance);
        safeDistance = Mathf.Clamp(safeDistance, 0, float.MaxValue);
    }
    public override void OnInitialize(object nodeMemory, SchemaAgent agent)
    {
        FleeMemory memory = (FleeMemory)nodeMemory;

        memory.agent = agent.GetComponent<NavMeshAgent>();
    }
    public override void OnNodeEnter(object nodeMemory, SchemaAgent agent)
    {
        FleeMemory memory = (FleeMemory)nodeMemory;
        GameObject enemyObject = enemy.value;

        if (enemyObject == null)
        {
            memory.point = Vector3.positiveInfinity;
            return;
        }

        Vector3 enemyPos = enemyObject.transform.position;
        Vector3 point = GetRandomPoint(agent, enemyPos);

        bool chosePoint = NavMesh.SamplePosition(point, out NavMeshHit hit, maxDistance, areaMask.mask);

        memory.enemy = enemyObject;

        if (!chosePoint)
            memory.point = Vector3.positiveInfinity;
        else
            memory.point = hit.position;
    }
    public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
    {
        FleeMemory memory = (FleeMemory)nodeMemory;

        if (memory.agent.SetDestination(memory.point))
        {
            if (!memory.agent.pathPending &&
            (memory.agent.remainingDistance <= memory.agent.stoppingDistance) &&
            (!memory.agent.hasPath || memory.agent.velocity.sqrMagnitude == 0f) &&
            Vector3.Distance(agent.transform.position, memory.enemy.transform.position) >= safeDistance)
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
    private Vector3 GetRandomPoint(SchemaAgent agent, Vector3 enemyPos)
    {
        Vector3 dir = (new Vector3(agent.transform.position.x, 0f, agent.transform.position.z) - enemyPos).normalized;
        Quaternion rot = Quaternion.LookRotation(dir, Vector3.up);

        float halfAngle = angle / 2f;
        float rad = halfAngle * Mathf.Deg2Rad;
        float halfPI = Mathf.PI / 2f;

        float yOffset = rot.eulerAngles.y * Mathf.Deg2Rad;

        float r = (maxDistance - minDistance) * Mathf.Sqrt(Random.value) + minDistance;
        float theta = Random.value * 2 * rad + (halfPI - rad);

        float x = agent.transform.position.x + r * Mathf.Cos(theta - yOffset);
        float y = agent.transform.position.z + r * Mathf.Sin(theta - yOffset);

        randomPoint = new Vector3(x, agent.transform.position.y, y);

        return randomPoint;
    }

#if UNITY_EDITOR
    public override void DrawGizmos(SchemaAgent agent)
    {
        if (!visualize) return;

        Debug.Log("hey");

        Vector3 dir = (new Vector3(agent.transform.position.x, 0f, agent.transform.position.z) - Vector3.zero).normalized;
        Quaternion rot = Quaternion.LookRotation(dir, Vector3.up);

        float halfAngle = angle / 2f;
        float rad = halfAngle * Mathf.Deg2Rad;
        float halfPI = Mathf.PI / 2f;

        float dx = Mathf.Cos(halfPI - rad);
        float dy = Mathf.Sin(halfPI - rad);

        Quaternion qa = Quaternion.Euler(0f, halfAngle, 0f);
        Quaternion qb = Quaternion.Euler(0f, -halfAngle, 0f);

        Vector3 from = rot * new Vector3(-dx, 0f, dy);

        Gizmos.DrawCube(randomPoint, Vector3.one * 0.1f);

        UnityEditor.Handles.DrawWireArc(agent.transform.position, Vector3.up, from, angle, maxDistance);
        UnityEditor.Handles.DrawWireArc(agent.transform.position, Vector3.up, from, angle, minDistance);
        Gizmos.DrawRay(agent.transform.position, qa * rot * Vector3.forward * maxDistance);
        Gizmos.DrawRay(agent.transform.position, qb * rot * Vector3.forward * maxDistance);

        Color handlesColor = UnityEditor.Handles.color;
        UnityEditor.Handles.color = Color.green;

        UnityEditor.Handles.DrawWireDisc(agent.transform.position, Vector3.up, safeDistance);

        UnityEditor.Handles.color = handlesColor;
    }
#endif
}
