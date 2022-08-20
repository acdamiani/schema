using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("d_SphereCollider Icon", true), LightIcon("SphereCollider Icon", true), Category("Physics"),
     Description("Casts a sphere along a ray and returns detailed information on what was hit")]
    public class SphereCast : Action
    {
        [Tooltip("Origin of the sphere")] public BlackboardEntrySelector<Vector3> origin;
        [Tooltip("Radius of the sphere")] public BlackboardEntrySelector<float> radius;

        [Tooltip("Direction in which to cast the sphere")]
        public BlackboardEntrySelector<Vector3> direction;

        [Tooltip("Max length of the cast")] public BlackboardEntrySelector<float> maxDistance =
            new BlackboardEntrySelector<float>(Mathf.Infinity);

        [Tooltip("Layer mask to use when casting the box")]
        public LayerMask layerMask;

        [Tooltip("Specifies whether this query should hit triggers")]
        public QueryTriggerInteraction queryTriggerInteraction;

        [Tooltip("BlackboardEntry to store a collection of the hit GameObjects, or the first hit object"), WriteOnly] 
        public BlackboardEntrySelector hit = new BlackboardEntrySelector();

        protected override void OnObjectEnable()
        {
            hit.ApplyFilters(typeof(GameObject), typeof(List<GameObject>), typeof(Transform), typeof(List<Transform>));
        }

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            RaycastHit[] raycastHits = Physics.SphereCastAll(
                origin.value,
                radius.value,
                direction.value,
                maxDistance.value,
                layerMask,
                queryTriggerInteraction
            );

            if (raycastHits.Length == 0)
                return NodeStatus.Failure;

            if (hit.entryType == typeof(GameObject) || hit.isDynamic)
                hit.value = raycastHits[0].collider.gameObject;
            else if (hit.entryType == typeof(List<GameObject>))
                hit.value = raycastHits.Select(rayHit => rayHit.collider.gameObject).ToList();
            else if (hit.entryType == typeof(Transform))
                hit.value = raycastHits[0].transform;
            else if (hit.entryType == typeof(List<Transform>))
                hit.value = raycastHits.Select(rayHit => rayHit.transform).ToList();

            return NodeStatus.Success;
        }
    }
}