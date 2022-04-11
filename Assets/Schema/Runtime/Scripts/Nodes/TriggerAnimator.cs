using UnityEngine;
using Schema;

[RequireAgentComponent(typeof(Animator))]
[DarkIcon("Dark/TriggerAnimator")]
[LightIcon("Light/TriggerAnimator")]
public class TriggerAnimator : Action
{
    class TriggerAnimatorMemory
    {
        public Animator animator;
    }
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
}
