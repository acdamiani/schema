using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("Nodes/d_Movement")]
    [LightIcon("Nodes/Movement")]
    [Category("Movement")]
    public class Patrol : Action
    {
        public ComponentSelector<NavMeshAgent> navMeshAgent;
        public List<BlackboardEntrySelector<Vector3>> points;

        public override void OnNodeEnter(object nodeMemory, SchemaAgent agent)
        {
            PatrolMemory memory = (PatrolMemory)nodeMemory;

            NavMeshAgent a = memory.agent = agent.GetComponent(navMeshAgent);

            if (a == null)
                return;

            memory.agent.SetDestination(points[memory.currentIndex].value);
        }

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            PatrolMemory memory = (PatrolMemory)nodeMemory;

            if (memory.agent == null)
                return NodeStatus.Failure;

            if (!memory.agent.pathPending &&
                memory.agent.remainingDistance <= memory.agent.stoppingDistance &&
                (!memory.agent.hasPath || memory.agent.velocity.sqrMagnitude == 0f))
            {
                memory.currentIndex++;
                memory.currentIndex = memory.currentIndex > points.Count - 1 ? 0 : memory.currentIndex;

                return NodeStatus.Success;
            }

            return NodeStatus.Running;
        }

        private class PatrolMemory
        {
            public NavMeshAgent agent;
            public int currentIndex;
        }
    }
}