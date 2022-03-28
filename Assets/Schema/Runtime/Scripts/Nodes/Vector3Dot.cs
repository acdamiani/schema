using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Schema.Runtime;

public class Vector3Dot : Action
{
    [Tooltip("LHS of the dot product")]
    public BlackboardEntrySelector<Vector3> vectorOne;
    [Tooltip("RHS of the dot product")]
    public BlackboardEntrySelector<Vector3> vectorTwo;
    [Tooltip("Blackboard variable to store the dot product in")]
    public BlackboardEntrySelector<float> angleKey;
    public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
    {
        float dot = Vector3.Dot(vectorOne.value, vectorTwo.value);

        angleKey.value = dot;

        return NodeStatus.Success;
    }
}
