using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("d_Transform Icon", true), LightIcon("Transform Icon", true), Category("Transform"),
     Description("Transform a given direction from world to local space")]
    public class InverseTransformDirection : Action
    {
        [Tooltip("Transform to operate on")] public ComponentSelector<Transform> transform;
        [Tooltip("Direction vector")] public BlackboardEntrySelector<Vector3> direction;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            Transform t = agent.GetComponent(transform);

            if (t == null)
                return NodeStatus.Failure;

            t.InverseTransformDirection(direction.value);

            return NodeStatus.Success;
        }
    }
}