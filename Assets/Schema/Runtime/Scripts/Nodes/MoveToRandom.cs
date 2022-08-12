using Schema;
using UnityEngine;
using UnityEngine.AI;

public class MoveToRandom : Action
{
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

        if (isRelative)
            memory.position += agent.transform.position;

        NavMeshAgent aiAgent = agent.GetComponent<NavMeshAgent>();
        //Disable agent
        if (aiAgent) aiAgent.enabled = false;
    }

    public override void OnNodeExit(object nodeMemory, SchemaAgent agent)
    {
        MoveToRandomMemory memory = (MoveToRandomMemory)nodeMemory;

        NavMeshAgent aiAgent = agent.GetComponent<NavMeshAgent>();

        //Reenable agent
        if (aiAgent)
            aiAgent.enabled = true;
    }

    public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
    {
        MoveToRandomMemory mem = (MoveToRandomMemory)nodeMemory;

        if (Vector3.SqrMagnitude(agent.transform.position - mem.position) < 0.1f)
        {
            Debug.Log("success");
            return NodeStatus.Success;
        }

        agent.transform.position = Vector3.MoveTowards(
            agent.transform.position,
            mem.position,
            speed * Time.deltaTime
        );

        Debug.Log("running");

        return NodeStatus.Running;
    }

    private class MoveToRandomMemory
    {
        public Vector3 position;
    }
}