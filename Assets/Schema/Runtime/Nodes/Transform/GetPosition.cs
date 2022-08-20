using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("d_Transform Icon", true), LightIcon("Transform Icon", true), Category("Transform")]
    public class GetPosition : Action
    {
        public ComponentSelector<Transform> transform;

        [WriteOnly, Tooltip("Key to store position in")] 
        public BlackboardEntrySelector<Vector3> positionKey;

        [Tooltip("When toggled, will use local position (relative to parent) instead of world position")]
        public bool local;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            Transform t = agent.GetComponent(transform);

            if (t == null)
                return NodeStatus.Failure;

            positionKey.value = local ? t.localPosition : t.position;

            return NodeStatus.Success;
        }
    }
}