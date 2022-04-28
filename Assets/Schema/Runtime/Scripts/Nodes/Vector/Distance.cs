using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Schema;

[Description("Find the distance between two Vector3 values")]
public class Distance : Action
{
    [Tooltip("Vector A")]
    public BlackboardEntrySelector<Vector3> vectorOne;
    [Tooltip("Vector B")]
    public BlackboardEntrySelector<Vector3> vectorTwo;
    [Tooltip("Blackboard variable to store the distance in"), WriteOnly]
    public BlackboardEntrySelector<float> distance;
    [Tooltip("Whether to get distance squred, which avoids the expensive square root operation")]
    public bool squared;
    public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
    {
        Vector3 diff = vectorOne.value - vectorTwo.value;
        float dist = squared ? diff.sqrMagnitude : diff.magnitude;

        distance.value = dist;

        return NodeStatus.Success;
    }
}
