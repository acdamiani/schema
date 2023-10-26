using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("d_Transform Icon", true), LightIcon("Transform Icon", true), Category("Transform"),
     Description("Get a child transform by an integer index")]
    public class GetChild : Action
    {
        [Tooltip("Transform to operate on")] public ComponentSelector<Transform> transform;

        [Tooltip("Index of the child to find")]
        public BlackboardEntrySelector<int> index;

        [Tooltip("Found transform"), WriteOnly] 
        public BlackboardEntrySelector<Transform> found;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            Transform t = agent.GetComponent(transform);

            if (t == null)
                return NodeStatus.Failure;

            if (index.value > t.childCount - 1)
            {
                found.value = null;
                return NodeStatus.Failure;
            }

            found.value = t = t.GetChild(index.value);

            if (t == null)
                return NodeStatus.Failure;

            return NodeStatus.Success;
        }
    }
}