using Schema;
using UnityEngine;

[RequireAgentComponent(typeof(Animator))]
public class AnimatorCrossFade : Action
{
    class AnimatorCrossFadeMemory
    {
        public Animator animator;
    }
    public BlackboardEntrySelector<string> stateName;
    public BlackboardEntrySelector<float> normalizedTransitionDuration;
    public BlackboardEntrySelector<int> layer;
    public BlackboardEntrySelector<float> normalizedTimeOffset;
    public BlackboardEntrySelector<float> normalizedTransitionTime;
    public override void OnInitialize(object nodeMemory, SchemaAgent agent)
    {
        ((AnimatorCrossFadeMemory)nodeMemory).animator = agent.GetComponent<Animator>();
    }
    public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
    {
        AnimatorCrossFadeMemory memory = (AnimatorCrossFadeMemory)nodeMemory;

        memory.animator.CrossFade(
            stateName.value,
            normalizedTransitionDuration.value,
            layer.value,
            normalizedTimeOffset.value,
            normalizedTransitionTime.value
        );

        return NodeStatus.Success;
    }
}