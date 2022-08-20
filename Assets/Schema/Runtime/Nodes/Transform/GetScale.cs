using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("d_Transform Icon", true), LightIcon("Transform Icon", true), Category("Transform")]
    public class GetScale : Action
    {
        public ComponentSelector<Transform> transform;

        [Tooltip("Key to store scale in")] public BlackboardEntrySelector<Vector3> scaleKey;

        [Tooltip("When toggled, will use local scale (relative to parent) instead of lossy scale")]
        public bool local;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            Transform t = agent.GetComponent(transform);

            if (t == null)
                return NodeStatus.Failure;

            if (local)
                scaleKey.value = t.transform.lossyScale;
            else
                scaleKey.value = t.transform.localScale;

            return NodeStatus.Success;
        }
    }
}