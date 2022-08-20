using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("d_Transform Icon", true)]
    [LightIcon("Transform Icon", true)]
    [Category("Transform")]
    [Description("Check if a transform is a child of another transform")]
    public class IsChild : Action
    {
        [Tooltip("Transform to operate on")] public ComponentSelector<Transform> transform;
        [Tooltip("Parent transform")] public ComponentSelector<Transform> parentTransform;

        [Tooltip("Whether the transform is a child of the parent transform")] [WriteOnly]
        public BlackboardEntrySelector<bool> isChild;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            Transform t1 = agent.GetComponent(transform);
            Transform t2 = agent.GetComponent(parentTransform);

            if (t1 == null || t2 == null)
                return NodeStatus.Failure;

            isChild.value = t1.IsChildOf(t2);

            return NodeStatus.Success;
        }
    }
}