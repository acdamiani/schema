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
        Vector3 cross = Vector3.Cross(vectorOne.value, vectorTwo.value);

        angleKey.value = cross;

        return NodeStatus.Success;
    }
}
