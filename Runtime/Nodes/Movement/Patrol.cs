using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("Nodes/d_Movement"), LightIcon("Nodes/Movement"), Category("Movement")]
    public class Patrol : Action
    {
        public ComponentSelector<NavMeshAgent> navMeshAgent;
        public List<BlackboardEntrySelector<Vector3>> points;

        public override void OnNodeEnter(object nodeMemory, SchemaAgent agent)
        {
            PatrolMemory memory = (PatrolMemory)nodeMemory;

            if (points.Count == 0)
                return;

            NavMeshAgent a = agent.GetComponent(navMeshAgent);

            if (a == null)
                return;

            a.SetDestination(points[memory.currentIndex].value);
        }

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            PatrolMemory memory = (PatrolMemory)nodeMemory;

            NavMeshAgent a = agent.GetComponent(navMeshAgent);

            if (points.Count == 0 || a == null)
                return NodeStatus.Failure;

            if (!a.pathPending &&
                a.remainingDistance <= a.stoppingDistance &&
                (!a.hasPath || a.velocity.sqrMagnitude == 0f))
            {
                memory.currentIndex++;
                memory.currentIndex = memory.currentIndex > points.Count - 1 ? 0 : memory.currentIndex;

                return NodeStatus.Success;
            }

            return NodeStatus.Running;
        }

        private class PatrolMemory
        {
            public int currentIndex;
        }
    }
}