using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("d_Transform Icon", true), LightIcon("Transform Icon", true), Category("Transform"),
     Description("Find a child of a Transform with a given name")]
    public class Find : Action
    {
        [Tooltip("Transform to operate on")] public ComponentSelector<Transform> transform;
        [Tooltip("Name of the child to find")] public BlackboardEntrySelector<string> childName;

        [Tooltip("Found transform"), WriteOnly] 
        public BlackboardEntrySelector<Transform> found;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            Transform t = agent.GetComponent(transform);

            if (t == null)
                return NodeStatus.Failure;

            t = t.Find(childName.value);

            found.value = t;

            if (t == null)
                return NodeStatus.Failure;

            return NodeStatus.Success;
        }
    }
}