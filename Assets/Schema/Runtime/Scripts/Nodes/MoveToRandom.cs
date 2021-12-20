using System.Collections;
using System.Collections.Generic;
using Schema.Runtime;
using UnityEngine;
using UnityEngine.AI;

[Category("Movement")]
public class MoveToRandom : Action
{
    class MoveToRandomMemory
    {
        public Vector3 position;
    }
    public int xMin;
    public int zMin;
    public int xMax;
    public int zMax;
    [Range(0, 100)]
    public float speed;
    [Tooltip("Whether the random position is relative to the current agent's position")]
    public bool isRelative = true;
    public override void OnNodeEnter(object nodeMemory, SchemaAgent agent)
    {
        MoveToRandomMemory memory = (MoveToRandomMemory)nodeMemory;
        memory.position = new Vector3(Random.Range(xMin, xMax), agent.transform.position.y, Random.Range(zMin, zMax));

        if (isRelative)
            memory.position += agent.transform.position;

        NavMeshAgent aiAgent = agent.GetComponent<NavMeshAgent>();
        //Disable agent
        if (aiAgent)
        {
            aiAgent.enabled = false;
        }
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
}
