using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Schema.Builtin.Conditionals
{
    [DarkIcon("Conditionals/ConeCheck")]
    public class ConeCheck : Conditional
    {
        public BlackboardEntrySelector<float> rayRange = new(10.0f);
        [Range(0f, 85f)] public float halfAngle = 15.0f;
        public float coneDirection;

        [Tooltip(
            "How many rays to send to check for colliding Game Objects. Increasing this value will reduce performance but will result in more accurate results for smaller objects.")]
        [Range(10, 100)]
        public int resolution = 10;

        [Tooltip("Visualize the cone for the selected agent with this tree attached")]
        public bool visualize = true;

        public bool precisionMode;
        public Vector3 offset;
        public BlackboardEntrySelector<GameObject> parentObject;
        public BlackboardEntrySelector<GameObject> gameObjectKey;

        [Tooltip("The tags to filter from. Only these tags will be considered when checking the cone")]
        public TagFilter tagFilter;

        public override bool Evaluate(object decoratorMemory, SchemaAgent agent)
        {
            GameObject go = TestCone(agent);
            if (go)
            {
                gameObjectKey.value = go;

                return true;
            }

            gameObjectKey.value = null;

            return false;
        }

        private GameObject TestCone(SchemaAgent agent)
        {
            if (precisionMode)
            {
                RayRepresentation[] rayRepresentations = GenerateRays(agent);
                Ray[] rays = new Ray[rayRepresentations.Length];

                for (int i = 0; i < rayRepresentations.Length; i++) rays[i] = (Ray)rayRepresentations[i];

                Dictionary<Ray, RaycastHit> hits = new Dictionary<Ray, RaycastHit>();
                hits = GetHitInfo(rays);

                foreach (RaycastHit hit in hits.Values)
                    if (tagFilter.tags.Contains(hit.transform.tag))
                        return hit.transform.gameObject;

                return null;
            }

            Quaternion offsetRotation = Quaternion.AngleAxis(coneDirection, agent.transform.right);
            Quaternion rotation = offsetRotation * agent.transform.rotation;
            float radius = Mathf.Tan(Mathf.Deg2Rad * halfAngle) * rayRange.value;
            Vector3 rotatedOffset = agent.transform.rotation * offset;

            Vector3 position = new Vector3(agent.transform.position.x + rotatedOffset.x,
                agent.transform.position.y + rotatedOffset.y,
                agent.transform.position.z + rotatedOffset.z);

            position += rotation * (Vector3.forward * (rayRange.value / 2f));

            Vector3 size = new Vector3(radius * 2, radius * 2, rayRange.value);

            Collider[] colliders = Physics.OverlapBox(
                position,
                size / 2f,
                rotation,
                -1,
                QueryTriggerInteraction.UseGlobal
            );

            foreach (Collider c in colliders)
                if (c.transform != agent.transform && PointInsideCone(
                        agent.transform.position + rotatedOffset, rotation * Vector3.forward, rayRange.value, radius,
                        c.transform.position
                    ) && tagFilter.tags.Contains(c.transform.tag))
                {
                    RaycastHit hit;
                    //Cast ray to check if object is blocked by something
                    if (Physics.Raycast(agent.transform.position + rotatedOffset,
                            (c.transform.position - (agent.transform.position + rotatedOffset)).normalized, out hit,
                            rayRange.value, -1, QueryTriggerInteraction.UseGlobal))
                        if (hit.collider == c)
                            return c.transform.gameObject;
                }


            return null;
        }

        private bool PointInsideCone(Vector3 tip, Vector3 normal, float height, float radius, Vector3 point)
        {
            float coneDist = Vector3.Dot(point - tip, normal);

            if (coneDist < 0 || coneDist > height)
                return false;

            float coneRadius = coneDist / height * radius;

            float orthDistance = Vector3.Magnitude(point - tip - coneDist * normal);

            return orthDistance < coneRadius;
        }

        private Dictionary<Ray, RaycastHit> GetHitInfo(Ray[] rays)
        {
            Dictionary<Ray, RaycastHit> hits = new Dictionary<Ray, RaycastHit>();
            for (int i = 0; i < rays.Length; i++)
            {
                RaycastHit hitInfo;

                if (Physics.Raycast(rays[i], out hitInfo, rayRange.value)) hits.Add(rays[i], hitInfo);
            }

            return hits;
        }

        private RayRepresentation[] GenerateRays(SchemaAgent agent)
        {
            Quaternion offsetRotation = Quaternion.AngleAxis(coneDirection, agent.transform.right);
            Quaternion rotation = offsetRotation * agent.transform.rotation;

            float radius = Mathf.Tan(Mathf.Deg2Rad * halfAngle);

            Vector2[] points = SunflowerDistribution(radius, resolution, 2);
            Vector3 rotatedOffset = agent.transform.rotation * offset;

            RayRepresentation[] rays = new RayRepresentation[points.Length];

            for (int i = 0; i < rays.Length; i++)
            {
                Vector3 modified = agent.transform.position + rotation * (Vector3.forward + (Vector3)points[i]);
                Vector3 direction = (modified + rotatedOffset - (agent.transform.position + rotatedOffset)) *
                                    rayRange.value;

                rays[i] = new RayRepresentation(agent.transform.position + rotatedOffset, direction);
            }

            return rays;
        }

        private Vector2[] SunflowerDistribution(float radius, int numPoints, int alpha)
        {
            int b = Mathf.RoundToInt(alpha * Mathf.Sqrt(numPoints));
            float phi = (Mathf.Sqrt(5) + 1f) / 2f;

            Vector2[] values = new Vector2[numPoints];

            for (int i = 1; i <= numPoints; i++)
            {
                float r = GetRadius(i, numPoints, b);
                float theta = 2 * Mathf.PI * i / Mathf.Pow(phi, 2);

                values[i - 1] = new Vector2(r * Mathf.Cos(theta), r * Mathf.Sin(theta)) * radius;
            }

            return values;
        }

        private float GetRadius(int k, int n, int b)
        {
            if (k > n - b)
                return 1.0f;
            return Mathf.Sqrt(k - 0.5f) / Mathf.Sqrt(n - (b + 1) / 2f);
        }

        public override GUIContent GetConditionalContent()
        {
            StringBuilder sb = new StringBuilder();

            if (gameObjectKey.isDynamic)
                sb.Append("If dynamic variable ");
            else if (gameObjectKey.entry != null)
                sb.Append("If variable ");
            else
                sb.Append("If ");

            sb.AppendFormat("<color=red>{0}</color> ",
                gameObjectKey.name);

            if (invert)
                sb.Append("is not inside cone");
            else
                sb.Append("is inside cone");

            return new GUIContent(sb.ToString());
        }
#if UNITY_EDITOR
        // public override void DrawGizmos(SchemaAgent agent)
        // {
        //     if (!visualize)
        //         return;

        //     if (precisionMode)
        //     {
        //         RayRepresentation[] rayRepresentations = GenerateRays(agent);
        //         Ray[] rays = new Ray[rayRepresentations.Length];

        //         for (int i = 0; i < rayRepresentations.Length; i++)
        //         {
        //             rays[i] = (Ray)rayRepresentations[i];
        //         }

        //         Dictionary<Ray, RaycastHit> hits = new Dictionary<Ray, RaycastHit>();
        //         hits = GetHitInfo(rays);

        //         for (int i = 0; i < rays.Length; i++)
        //         {
        //             RayRepresentation rayRepresentation = rayRepresentations[i];
        //             Ray ray = rays[i];

        //             if (hits.ContainsKey(ray) && tagFilter.tags.Contains(hits[ray].transform.tag))
        //                 Gizmos.color = Color.green;
        //             else
        //                 Gizmos.color = Color.white;

        //             Gizmos.DrawRay(rayRepresentation.start, rayRepresentation.direction);
        //         }
        //     }
        //     else
        //     {
        //         DrawCone(agent, TestCone(agent));
        //     }
        // }
        private void DrawCone(SchemaAgent agent, bool hit)
        {
            Quaternion offsetRotation = Quaternion.AngleAxis(coneDirection, agent.transform.right);
            Quaternion rotation = offsetRotation * agent.transform.rotation;

            Vector3 rotatedOffset = agent.transform.rotation * offset;

            float radius = Mathf.Tan(Mathf.Deg2Rad * halfAngle) * rayRange.value;

            Vector3 normal = rotation * Vector3.forward;
            Vector3 tip = agent.transform.position + rotatedOffset;
            Vector3 topNoRotate = Vector3.forward * rayRange.value;
            Vector3 topCenter = rotation * topNoRotate;

            Handles.color = hit ? Color.green : Color.white;

            Handles.zTest = CompareFunction.LessEqual;
            Handles.DrawLine(tip,
                agent.transform.position + rotatedOffset + rotation * (topNoRotate + Vector3.up * radius), 2f);
            Handles.DrawLine(tip,
                agent.transform.position + rotatedOffset + rotation * (topNoRotate + Vector3.down * radius), 2f);
            Handles.DrawLine(tip,
                agent.transform.position + rotatedOffset + rotation * (topNoRotate + Vector3.left * radius), 2f);
            Handles.DrawLine(tip,
                agent.transform.position + rotatedOffset + rotation * (topNoRotate + Vector3.right * radius), 2f);
            Handles.DrawWireDisc(agent.transform.position + rotatedOffset + topCenter, normal, radius, 2f);
        }
#endif
        private class RayRepresentation
        {
            public readonly Vector3 start;
            public Vector3 direction;

            public RayRepresentation(Vector3 start, Vector3 direction)
            {
                this.start = start;
                this.direction = direction;
            }

            public static explicit operator Ray(RayRepresentation rayRepresentation)
            {
                return new Ray(rayRepresentation.start, rayRepresentation.direction.normalized);
            }
        }
    }
}