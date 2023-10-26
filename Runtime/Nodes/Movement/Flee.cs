using UnityEngine;
using UnityEngine.AI;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("Nodes/d_Movement"), LightIcon("Nodes/Movement"), Category("Movemenet")]
    public class Flee : Action
    {
        public ComponentSelector<NavMeshAgent> navMeshAgent;
        [Tooltip("The enemy GameObject")] public BlackboardEntrySelector<GameObject> enemy;

        [Tooltip("The NavMesh surfaces the agent is allowed to choose points on")]
        public NavMeshAreaMask areaMask;

        [Range(10f, 180f), Tooltip("The maximum angle to choose a point in front of the agent")] 
        public float angle = 45f;

        [Min(1f), Tooltip("The maximum distance to choose a point")] 
        public float maxDistance = 10f;

        [Tooltip("The minimum distance to choose a point")]
        public float minDistance = 2f;

        [Tooltip("The distance away from the enemy that the agent considers \"safe.\"")]
        public float safeDistance = 25f;

        private Vector3 randomPoint;

        private void OnValidate()
        {
            maxDistance = Mathf.Clamp(maxDistance, 0, float.MaxValue);
            minDistance = Mathf.Clamp(minDistance, 0, maxDistance);
            safeDistance = Mathf.Clamp(safeDistance, 0, float.MaxValue);
        }

        public override void OnNodeEnter(object nodeMemory, SchemaAgent agent)
        {
            FleeMemory memory = (FleeMemory)nodeMemory;
            GameObject enemyObject = enemy.value;

            NavMeshAgent a = memory.agent = agent.GetComponent(navMeshAgent);

            if (a == null)
                return;

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

            if (memory.agent == null)
                return NodeStatus.Failure;

            if (memory.agent.SetDestination(memory.point))
            {
                if (!memory.agent.pathPending &&
                    memory.agent.remainingDistance <= memory.agent.stoppingDistance &&
                    (!memory.agent.hasPath || memory.agent.velocity.sqrMagnitude == 0f) &&
                    Vector3.Distance(agent.transform.position, memory.enemy.transform.position) >= safeDistance)
                    return NodeStatus.Success;

                return NodeStatus.Running;
            }

            return NodeStatus.Failure;
        }

        private Vector3 GetRandomPoint(SchemaAgent agent, Vector3 enemyPos)
        {
            Vector3 dir = (new Vector3(agent.transform.position.x, 0f, agent.transform.position.z) - enemyPos)
                .normalized;
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

        private class FleeMemory
        {
            public NavMeshAgent agent;
            public GameObject enemy;
            public Vector3 point;
        }
    }
}