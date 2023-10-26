using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("d_Transform Icon", true), LightIcon("Transform Icon", true), Category("Transform"),
     Description("Get the sibling index of a Transform")]
    public class GetSiblingIndex : Action
    {
        [Tooltip("Transform to operate on")] public ComponentSelector<Transform> transform;

        [Tooltip("Index of the sibling Transform"), WriteOnly] 
        public BlackboardEntrySelector<int> siblingIndex;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            Transform t = agent.GetComponent(transform);

            if (t == null)
                return NodeStatus.Failure;

            siblingIndex.value = t.GetSiblingIndex();

            return NodeStatus.Success;
        }
    }
}