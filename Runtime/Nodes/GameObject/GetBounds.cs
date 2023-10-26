using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("d_MeshFilter Icon", true), LightIcon("MeshFilter Icon", true), Category("GameObject")]
    public class GetBounds : Action
    {
        [Tooltip("When toggled, will get the bounds of the current SchemaAgent")]
        public bool useSelf;

        [Tooltip("The GameObject to get the bounds from")]
        public BlackboardEntrySelector<GameObject> gameObject;

        [Tooltip("Blackboard key to store the boudns of the object in")]
        public BlackboardEntrySelector<Vector3> boundsKey;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            GameObject obj = useSelf ? agent.gameObject : gameObject.value;

            if (obj == null || boundsKey == null)
                return NodeStatus.Failure;

            Mesh mesh = obj.GetComponent<MeshFilter>()?.mesh;

            if (mesh == null)
                return NodeStatus.Failure;

            Vector3 objectSize = Vector3.Scale(obj.transform.localScale, mesh.bounds.size);

            boundsKey.value = objectSize;

            return NodeStatus.Success;
        }
    }
}