using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Schema;

[RequireAgentComponent(typeof(Animator))]
public class SetAnimatorLookAt : Action
{
    class SetAnimatorLayerWeightMemory
    {
        public Animator animator;
    }
    [Tooltip("The position to look at")]
    public Vector3 lookAtPosition;
    public override void OnInitialize(object nodeMemory, SchemaAgent agent)
    {
        SetAnimatorLayerWeightMemory memory = (SetAnimatorLayerWeightMemory)nodeMemory;

        memory.animator = agent.GetComponent<Animator>();
    }
    public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
    {
        SetAnimatorLayerWeightMemory memory = (SetAnimatorLayerWeightMemory)nodeMemory;

        memory.animator.SetLookAtPosition(lookAtPosition);

        return NodeStatus.Success;
    }
}
