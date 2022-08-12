using Schema;
using UnityEngine;

[RequireAgentComponent(typeof(Animator))]
[DarkIcon("Dark/TriggerAnimator")]
[LightIcon("Light/TriggerAnimator")]
public class TriggerAnimator : Action
{
    public string trigger;

    public override void OnInitialize(object nodeMemory, SchemaAgent agent)
    {
        TriggerAnimatorMemory memory = (TriggerAnimatorMemory)nodeMemory;

        memory.animator = agent.GetComponent<Animator>();
    }

    public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
    {
        TriggerAnimatorMemory memory = (TriggerAnimatorMemory)nodeMemory;

        memory.animator.SetTrigger(trigger);

        return NodeStatus.Success;
    }

    private class TriggerAnimatorMemory
    {
        public Animator animator;
    }
}