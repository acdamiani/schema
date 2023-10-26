using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("d_Transform Icon", true), LightIcon("Transform Icon", true), Category("Transform")]
    public class GetRotation : Action
    {
        public ComponentSelector<Transform> transform;

        [Tooltip("Get rotation in euler angles")]
        public bool eulerAngles = true;


        [Tooltip("Key to store euler angles of rotation in")]
        public BlackboardEntrySelector<Vector3> eulerKey;

        [Tooltip("Key to store rotation as Quaternion in")]
        public BlackboardEntrySelector<Quaternion> quaternionKey;

        [Tooltip("When toggled, will use local rotation (relative to parent) instead of world position")]
        public bool local;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            Transform t = agent.GetComponent(transform);

            if (t == null)
                return NodeStatus.Failure;

            if (local)
            {
                if (eulerAngles)
                    eulerKey.value = t.localEulerAngles;
                else
                    quaternionKey.value = t.localRotation;
            }
            else
            {
                if (eulerAngles)
                    eulerKey.value = t.eulerAngles;
                else
                    quaternionKey.value = t.rotation;
            }

            return NodeStatus.Success;
        }
    }
}