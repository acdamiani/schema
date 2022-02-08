using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Schema.Runtime;

public class Vector3Angle : Action
{
    [Tooltip("The first vector to get the angle between")]
    public BlackboardEntrySelector<Vector3> vectorOne;
    [Tooltip("The second vector to get the angle between")]
    public BlackboardEntrySelector<Vector3> vectorTwo;
    [Tooltip("Blackboard variable to store the angle in")]
    public BlackboardEntrySelector<float> angleKey;
    [Tooltip("Get the signed angle between Vectors")]
    public bool signed;
    [Tooltip("Convert result to radians")]
    public bool radians;
    public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
    {
        float angle;

        Vector3 v1 = agent.blackboard.GetType(vectorOne) == typeof(Vector3) ?
            agent.blackboard.GetValue<Vector3>(vectorOne) :
            (Vector3)agent.blackboard.GetValue<Vector2>(vectorOne);

        Vector3 v2 = agent.blackboard.GetType(vectorTwo) == typeof(Vector3) ?
            agent.blackboard.GetValue<Vector3>(vectorTwo) :
            (Vector3)agent.blackboard.GetValue<Vector2>(vectorTwo);

        if (signed)
            angle = Vector3.SignedAngle(v1, v2, Vector3.up);
        else
            angle = Vector3.Angle(v1, v2);

        angle = radians ? Mathf.Deg2Rad * angle : angle;

        agent.blackboard.SetValue<float>(angleKey, angle);

        return NodeStatus.Success;
    }
}
