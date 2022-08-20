using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("d_Transform Icon", true), LightIcon("Transform Icon", true), Category("Transform"),
     Description("Unparent all children of a Transform")]
    public class DetachChildren : Action
    {
        [Tooltip("Transform to detach from children")]
        public ComponentSelector<Transform> transform;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            Transform t = agent.GetComponent(transform);

            if (t == null)
                return NodeStatus.Failure;

            t.DetachChildren();

            return NodeStatus.Success;
        }
    }
}