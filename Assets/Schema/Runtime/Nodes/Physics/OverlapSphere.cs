using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("d_SphereCollider Icon", true), LightIcon("SphereCollider Icon", true), Category("Physics"),
     Description("Gets colliders hit by a sphere positioned in the world")]
    public class OverlapSphere : Action
    {
        [Tooltip("Position of the sphere")] public BlackboardEntrySelector<Vector3> position;
        [Tooltip("Radius of the sphere")] public BlackboardEntrySelector<float> radius;

        [Tooltip("Layer mask to use when casting the box")]
        public LayerMask layerMask;

        [Tooltip("Specifies whether this query should hit triggers")]
        public QueryTriggerInteraction queryTriggerInteraction;

        [Tooltip("BlackboardEntry to store a collection of the hit GameObjects, or the first hit GameObject"),
         WriteOnly]
        public BlackboardEntrySelector hit = new BlackboardEntrySelector();

        protected override void OnObjectEnable()
        {
            hit.ApplyFilters(typeof(GameObject), typeof(List<GameObject>), typeof(Transform), typeof(List<Transform>));
        }

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            Collider[] colliders =
                Physics.OverlapSphere(position.value, radius.value, layerMask, queryTriggerInteraction);

            if (colliders.Length == 0)
                return NodeStatus.Failure;

            if (hit.entryType == typeof(GameObject) || hit.isDynamic)
                hit.value = colliders[0].gameObject;
            else if (hit.entryType == typeof(List<GameObject>))
                hit.value = colliders.Select(collider => collider.gameObject).ToList();
            else if (hit.entryType == typeof(Transform))
                hit.value = colliders[0].transform;
            else if (hit.entryType == typeof(List<Transform>))
                hit.value = colliders.Select(collider => collider.transform).ToList();

            return NodeStatus.Success;
        }
    }
}