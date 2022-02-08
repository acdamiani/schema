using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Schema.Runtime;

public class Vector3Cross : Action
{
    [Tooltip("LHS of the cross product")]
    public BlackboardEntrySelector<Vector3> vectorOne;
    [Tooltip("RHS of the cross product")]
    public BlackboardEntrySelector<Vector3> vectorTwo;
    [Tooltip("Blackboard variable to store the cross product in")]
    public BlackboardEntrySelector<Vector3> angleKey;
    public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
    {
        Vector3 v1 = agent.blackboard.GetType(vectorOne) == typeof(Vector3) ?
            agent.blackboard.GetValue<Vector3>(vectorOne) :
            (Vector3)agent.blackboard.GetValue<Vector2>(vectorOne);

        Vector3 v2 = agent.blackboard.GetType(vectorTwo) == typeof(Vector3) ?
            agent.blackboard.GetValue<Vector3>(vectorTwo) :
            (Vector3)agent.blackboard.GetValue<Vector2>(vectorTwo);

        Vector3 cross = Vector3.Cross(v1, v2);

        agent.blackboard.SetValue<Vector3>(angleKey, cross);

        return NodeStatus.Success;
    }
}
