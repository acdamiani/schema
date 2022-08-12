using Schema;
using UnityEngine;

[RequireAgentComponent(typeof(Animator))]
[DarkIcon("Dark/PlayAnimation")]
[LightIcon("Light/PlayAnimation")]
public class PlayAnimation : Action
{
    public string animationName;
    public int layer;

    public override void OnNodeEnter(object nodeMemory, SchemaAgent agent)
    {
        PlayAnimationMemory memory = (PlayAnimationMemory)nodeMemory;

        if (memory.animator == null)
            memory.animator = agent.GetComponent<Animator>();

        memory.stateInfo = memory.animator.GetCurrentAnimatorStateInfo(layer);
        memory.animator.Play(animationName, layer);

        memory.hasPlayed = false;
    }

    public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
    {
        PlayAnimationMemory memory = (PlayAnimationMemory)nodeMemory;
        AnimatorStateInfo info = memory.animator.GetCurrentAnimatorStateInfo(layer);

        Debug.Log(info.normalizedTime < 1);

        if (!memory.hasPlayed || (info.IsName(animationName) && info.normalizedTime < 1))
        {
            memory.hasPlayed = true;
            //Playing animation
            return NodeStatus.Running;
        }

        memory.animator.Play(memory.stateInfo.fullPathHash, layer);
        return NodeStatus.Success;
    }

    private class PlayAnimationMemory
    {
        public Animator animator;

        //When calling Animator.Play, there is a frame gap between the method being called and the animation actually running
        //This means that we should not do checks for animations on the same frame as the setup occuring in OnNodeEnter
        //This makes this variable necessary, to signify whether the animation has had a chance to play
        public bool hasPlayed;
        public AnimatorStateInfo stateInfo;
    }
}