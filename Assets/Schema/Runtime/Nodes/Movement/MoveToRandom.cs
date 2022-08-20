using UnityEngine;
using UnityEngine.AI;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("Nodes/d_Movement"), LightIcon("Nodes/Movement"), Category("Movement")]
    public class MoveToRandom : Action
    {
        public ComponentSelector<Transform> transform;
        public Vector2 x;
        public Vector2 y;
        public Vector2 z;

        [Range(0, 100)] public float speed;

        [Tooltip("Whether the random position is relative to the current agent's position")]
        public bool isRelative = true;

        public override void OnNodeEnter(object nodeMemory, SchemaAgent agent)
        {
            MoveToRandomMemory memory = (MoveToRandomMemory)nodeMemory;
            memory.position = new Vector3(Random.Range(x.x, x.y), Random.Range(y.x, y.y), Random.Range(z.x, z.y));

            Transform t = agent.GetComponent(transform);

            if (t == null)
                return;

            if (isRelative)
                memory.position += agent.transform.position;

            NavMeshAgent a = t.GetComponent<NavMeshAgent>();
            //Disable agent
            if (a) a.enabled = false;
        }

        public override void OnNodeExit(object nodeMemory, SchemaAgent agent)
        {
            MoveToRandomMemory memory = (MoveToRandomMemory)nodeMemory;

            Transform t = agent.GetComponent(transform);

            if (t == null)
                return;

            NavMeshAgent a = agent.GetComponent<NavMeshAgent>();

            //Reenable agent
            if (a)
                a.enabled = true;
        }

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            MoveToRandomMemory mem = (MoveToRandomMemory)nodeMemory;

            Transform t = agent.GetComponent(transform);

            if (t == null)
                return NodeStatus.Failure;

            if (Vector3.SqrMagnitude(agent.transform.position - mem.position) < 0.1f)
                return NodeStatus.Success;

            agent.transform.position = Vector3.MoveTowards(
                agent.transform.position,
                mem.position,
                speed * Time.deltaTime
            );

            return NodeStatus.Running;
        }

        private class MoveToRandomMemory
        {
            public Vector3 position;
        }
    }
}