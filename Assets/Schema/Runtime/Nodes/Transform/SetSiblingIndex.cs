using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("d_Transform Icon", true), LightIcon("Transform Icon", true), Category("Transform"),
     Description("Set the sibling index of a Transform")]
    public class SetSiblingIndex : Action
    {
        [Tooltip("Transform to operate on")] public ComponentSelector<Transform> transform;
        [Tooltip("Sibling index")] public BlackboardEntrySelector<int> siblingIndex;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            Transform t = agent.GetComponent(transform);

            if (t == null)
                return NodeStatus.Failure;

            t.SetSiblingIndex(siblingIndex.value);

            return NodeStatus.Success;
        }
    }
}