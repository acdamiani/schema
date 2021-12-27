using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Schema.Runtime;

public class Distance : Action
{
    [Tooltip("Vector A")]
    public BlackboardVector vectorOne;
    [Tooltip("Vector B")]
    public BlackboardVector vectorTwo;
    [Tooltip("Blackboard variable to store the distance in")]
    public BlackboardFloat distance;
    [Tooltip("Whether to get distance squred, which avoids the expensive sqrt operation")]
    public bool squared;
    public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
    {
        Vector3 v1 = agent.blackboard.GetType(vectorOne) == typeof(Vector3) ?
            agent.blackboard.GetValue<Vector3>(vectorOne) :
            (Vector3)agent.blackboard.GetValue<Vector2>(vectorOne);

        Vector3 v2 = agent.blackboard.GetType(vectorTwo) == typeof(Vector3) ?
            agent.blackboard.GetValue<Vector3>(vectorTwo) :
            (Vector3)agent.blackboard.GetValue<Vector2>(vectorTwo);

        Vector3 diff = v1 - v2;
        float dist = squared ? diff.sqrMagnitude : diff.magnitude;

        agent.blackboard.SetValue<float>(distance, dist);

        return NodeStatus.Success;
    }
}
