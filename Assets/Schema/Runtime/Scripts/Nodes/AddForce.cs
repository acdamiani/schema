using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Schema.Runtime;

[RequireAgentComponent(typeof(Rigidbody))]
public class AddForce : Action
{
    public Vector3 forceVector;
    public ForceMode forceMode;
    class AddForceMemory
    {
        public Rigidbody rigidbody;
    }
    public override void OnInitialize(object nodeMemory, SchemaAgent agent)
    {
        AddForceMemory memory = (AddForceMemory)nodeMemory;

        memory.rigidbody = agent.GetComponent<Rigidbody>();
    }
    public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
    {
        AddForceMemory memory = (AddForceMemory)nodeMemory;

        if (memory.rigidbody != null)
        {
            memory.rigidbody.AddForce(forceVector, forceMode);
            return NodeStatus.Success;
        }
        else
        {
            return NodeStatus.Failure;
        }
    }
}
