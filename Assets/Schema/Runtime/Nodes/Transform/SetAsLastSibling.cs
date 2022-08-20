using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("d_Transform Icon", true), LightIcon("Transform Icon", true), Category("Transform"),
     Description("Set a Transform to be last in its local transform list")]
    public class SetAsLastSibling : Action
    {
        [Tooltip("Transform to operate on")] public ComponentSelector<Transform> transform;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            Transform t = agent.GetComponent(transform);

            if (t == null)
                return NodeStatus.Failure;

            t.SetAsLastSibling();

            return NodeStatus.Success;
        }
    }
}