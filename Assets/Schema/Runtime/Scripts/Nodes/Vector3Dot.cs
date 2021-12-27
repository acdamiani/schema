using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Schema.Runtime;

public class Vector3Dot : Action
{
    [Tooltip("LHS of the dot product")]
    public BlackboardVector vectorOne;
    [Tooltip("RHS of the dot product")]
    public BlackboardVector vectorTwo;
    [Tooltip("Blackboard variable to store the dot product in")]
    public BlackboardFloat angleKey;
    public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
    {
        Vector3 v1 = agent.blackboard.GetType(vectorOne) == typeof(Vector3) ?
            agent.blackboard.GetValue<Vector3>(vectorOne) :
            (Vector3)agent.blackboard.GetValue<Vector2>(vectorOne);

        Vector3 v2 = agent.blackboard.GetType(vectorTwo) == typeof(Vector3) ?
            agent.blackboard.GetValue<Vector3>(vectorTwo) :
            (Vector3)agent.blackboard.GetValue<Vector2>(vectorTwo);

        float dot = Vector3.Dot(v1, v2);

        agent.blackboard.SetValue<float>(angleKey, dot);

        return NodeStatus.Success;
    }
}
