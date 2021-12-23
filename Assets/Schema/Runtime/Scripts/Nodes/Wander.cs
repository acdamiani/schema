using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Schema.Runtime;

[DarkIcon("Dark/Wander")]
[LightIcon("Light/Wander")]
[RequireAgentComponent(typeof(NavMeshAgent))]
public class Wander : Action
{
    [Tooltip("The distance an agent will travel in any direction")]
    public float distance = 1f;
    public NavMeshAreaMask navmeshAreaMask;
    public bool visualize;
    class WanderMemory
    {
        public NavMeshAgent agent;
        public Vector3 destination;
    }
    public override void OnInitialize(object nodeMemory, SchemaAgent agent)
    {
        WanderMemory memory = (WanderMemory)nodeMemory;

        memory.agent = agent.GetComponent<NavMeshAgent>();
    }
    public override void OnNodeEnter(object nodeMemory, SchemaAgent agent)
    {
        WanderMemory memory = (WanderMemory)nodeMemory;

        //Taken from https://forum.unity.com/threads/solved-random-wander-ai-using-navmesh.327950/
        Vector3 randomDirection = Random.insideUnitSphere * distance;
        randomDirection += agent.transform.position;

        NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, distance, navmeshAreaMask.mask);

        memory.destination = hit.position;
    }
    public override void OnNodeExit(object nodeMemory, SchemaAgent agent)
    {
        WanderMemory memory = (WanderMemory)nodeMemory;
        memory.agent.ResetPath();
    }
    public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
    {
        WanderMemory memory = (WanderMemory)nodeMemory;

        if (memory.agent.SetDestination(memory.destination) && memory.agent.pathStatus == NavMeshPathStatus.PathComplete)
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
    public override void DrawGizmos(SchemaAgent agent)
    {
        if (!visualize) return;

        Color gizmosColor = Gizmos.color;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(agent.transform.position, distance);
        Gizmos.color = gizmosColor;
    }
}
