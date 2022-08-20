using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("d_BoxCollider Icon", true), LightIcon("BoxCollider Icon", true), Category("Physics"),
     Description("Gets colliders hit by a box positioned in the world")]
    public class OverlapBox : Action
    {
        [Tooltip("Center of the box")] public BlackboardEntrySelector<Vector3> center;

        [Tooltip("Half of the size of the box in each dimension")]
        public BlackboardEntrySelector<Vector3> halfExtents;

        [Tooltip("Rotation of the box")] public BlackboardEntrySelector<Quaternion> orientation;

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

            ;
        }

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            Collider[] colliders = Physics.OverlapBox(center.value, halfExtents.value, orientation.value, layerMask,
                queryTriggerInteraction);

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