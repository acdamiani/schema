using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("Nodes/d_Movement"), LightIcon("Nodes/Movement"), Category("Movement")]
    public class MoveToDirect : Action
    {
        public float speed = 1;
        public bool rotateTowardsTarget;
        public BlackboardEntrySelector<Vector3> point;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            if (rotateTowardsTarget)
            {
                Vector3 target = (agent.transform.position - point.value).normalized;
                Quaternion rotation = agent.transform.rotation * Quaternion.LookRotation(target);
                agent.transform.rotation = rotation;
            }

            if (Vector3.SqrMagnitude(agent.transform.position - point.value) < 0.1f) return NodeStatus.Success;
            agent.transform.position =
                Vector3.MoveTowards(agent.transform.position, point.value, speed * Time.deltaTime);
            return NodeStatus.Running;
        }
    }
}