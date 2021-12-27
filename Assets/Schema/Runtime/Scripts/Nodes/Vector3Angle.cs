using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Schema.Runtime;

public class Vector3Angle : Action
{
    class Vector3AngleMemory
    {
        public BlackboardData data;
    }
    [Tooltip("The first vector to get the angle between")]
    public BlackboardVector vectorOne;
    [Tooltip("The second vector to get the angle between")]
    public BlackboardVector vectorTwo;
    [Tooltip("Blackboard variable to store the angle in")]
    public BlackboardFloat angle;
    [Tooltip("Get the signed angle between Vectors")]
    public bool signed;
    [Tooltip("Convert result to radians")]
    public bool radians;

    public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
    {
        Vector3AngleMemory memory = (Vector3AngleMemory)nodeMemory;

        if (signed)
        {

        }

        return NodeStatus.Success;
    }
}
