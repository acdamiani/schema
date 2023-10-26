using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("d_Transform Icon", true), LightIcon("Transform Icon", true), Category("Transform"),
     Description("Set the parent of a Transform")]
    public class SetParent : Action
    {
        [Tooltip("Transform to operate on")] public ComponentSelector<Transform> transform;

        [Tooltip("Transform to use as parent")]
        public ComponentSelector<Transform> parentTransform;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            Transform t1 = agent.GetComponent(transform);
            Transform t2 = agent.GetComponent(parentTransform);

            if (t1 == null || t2 == null)
                return NodeStatus.Failure;

            t1.SetParent(t2);

            return NodeStatus.Success;
        }
    }
}