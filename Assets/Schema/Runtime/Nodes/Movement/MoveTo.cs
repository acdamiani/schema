using UnityEngine;
using UnityEngine.AI;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("Nodes/d_Movement"), LightIcon("Light/Movement"), Category("Movement")]
    public class MoveTo : Action
    {
        [Tooltip("The agent to move")] public ComponentSelector<NavMeshAgent> navMeshAgent;
        [Tooltip("The position to move to")] public BlackboardEntrySelector<Vector3> selector;

        public override void OnNodeExit(object nodeMemory, SchemaAgent agent)
        {
            NavMeshAgent a = agent.GetComponent(navMeshAgent);

            if (a == null)
                return;

            a.ResetPath();
        }

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            NavMeshAgent a = agent.GetComponent(navMeshAgent);

            if (a == null)
                return NodeStatus.Failure;

            if (a.SetDestination(selector.value))
            {
                if (!a.pathPending &&
                    a.remainingDistance <= a.stoppingDistance &&
                    (!a.hasPath || a.velocity.sqrMagnitude == 0f))
                    return NodeStatus.Success;

                return NodeStatus.Running;
            }

            return NodeStatus.Failure;
        }
    }
}