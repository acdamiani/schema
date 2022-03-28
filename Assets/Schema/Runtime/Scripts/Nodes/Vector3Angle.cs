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

        if (signed)
            angle = Vector3.SignedAngle(vectorOne.value, vectorTwo.value, Vector3.up);
        else
            angle = Vector3.Angle(vectorOne.value, vectorTwo.value);

        angle = radians ? Mathf.Deg2Rad * angle : angle;

        angleKey.value = angle;

        return NodeStatus.Success;
    }
}
