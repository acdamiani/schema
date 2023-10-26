using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("d_Animator Icon", true), LightIcon("Animator Icon", true), Category("Animator")]
    public class TriggerAnimator : Action
    {
        public ComponentSelector<Animator> animator;
        public string trigger;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            Animator a = agent.GetComponent(animator);

            if (a == null)
                return NodeStatus.Failure;

            a.SetTrigger(trigger);

            return NodeStatus.Success;
        }
    }
}