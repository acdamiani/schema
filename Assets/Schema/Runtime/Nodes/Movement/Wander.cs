using UnityEngine;
using UnityEngine.AI;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("Nodes/d_Movement")]
    [LightIcon("Light/Movement")]
    [Category("Movement")]
    public class Wander : Action
    {
        public ComponentSelector<NavMeshAgent> navMeshAgent;

        [Tooltip("The distance an agent will travel in any direction")]
        public float distance = 1f;

        public NavMeshAreaMask navmeshAreaMask;

        [Tooltip("View the wander sphere in the editor")]
        public bool visualize;

        public override void OnNodeEnter(object nodeMemory, SchemaAgent agent)
        {
            WanderMemory memory = (WanderMemory)nodeMemory;

            NavMeshAgent a = memory.agent = agent.GetComponent(navMeshAgent);

            if (a == null)
                return;

            Vector3 randomDirection = Random.insideUnitSphere * distance;
            randomDirection += a.transform.position;

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

            NavMeshAgent a = agent.GetComponent(navMeshAgent);

            if (a == null)
                return NodeStatus.Failure;

            if (memory.agent.SetDestination(memory.destination) &&
                memory.agent.pathStatus == NavMeshPathStatus.PathComplete)
            {
                if (!memory.agent.pathPending &&
                    memory.agent.remainingDistance <= memory.agent.stoppingDistance &&
                    (!memory.agent.hasPath || memory.agent.velocity.sqrMagnitude == 0f))
                    return NodeStatus.Success;

                return NodeStatus.Running;
            }

            return NodeStatus.Failure;
        }

        public override void DoNodeGizmos(SchemaAgent agent)
        {
            if (!visualize)
                return;

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(agent.transform.position, distance);
            Gizmos.color = Color.white;
        }

        private class WanderMemory
        {
            public NavMeshAgent agent;
            public Vector3 destination;
        }
    }
}