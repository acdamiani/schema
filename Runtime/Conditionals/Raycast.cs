using System.Linq;
using System.Text;
using UnityEngine;

namespace Schema.Builtin.Conditionals
{
    [Description("Cast a ray absolutely or dynamically towards an object or point"), DarkIcon("Conditionals/d_Raycast"),
     LightIcon("Conditionals/Raycast")]
    public class Raycast : Conditional
    {
        public enum RaycastType
        {
            Absolute,
            Dynamic
        }

        [Tooltip("Offset of the ray")] public Vector3 offset;

        [Tooltip("Direction of the ray, in euler angles")]
        public Vector3 direction;

        [Tooltip("Maximum distance of the ray")]
        public float maxDistance;

        [Tooltip("Point to cast ray towards")] public BlackboardEntrySelector<Vector3> point;
        public RaycastType type;
        public TagFilter tagFilter;

        [Tooltip("Visualize the ray in the editor")]
        public bool visualize;

        public override bool Evaluate(object decoratorMemory, SchemaAgent agent)
        {
            return TestCone(agent);
        }

        public override void DoConditionalGizmos(SchemaAgent agent)
        {
            if (type == RaycastType.Dynamic || !visualize) return;

            Color col = Gizmos.color;
            Vector3 rotatedOffset = agent.transform.rotation * offset;
            Vector3 rotatedDir = agent.transform.rotation * (Quaternion.Euler(direction) * Vector3.forward);

            if (TestCone(agent))
                Gizmos.color = Color.green;
            else
                Gizmos.color = Color.white;

            Gizmos.DrawRay(agent.transform.position + rotatedOffset, rotatedDir * maxDistance);

            Gizmos.color = col;
        }

        private bool TestCone(SchemaAgent agent)
        {
            RaycastHit[] hits;

            if (type == RaycastType.Absolute)
            {
                Vector3 rotatedOffset = agent.transform.rotation * offset;
                Vector3 rotatedDir = agent.transform.rotation * (Quaternion.Euler(direction) * Vector3.forward);

                hits = Physics.RaycastAll(agent.transform.position + rotatedOffset, rotatedDir, maxDistance);
            }
            else
            {
                Vector3 p = point.value;
                hits = Physics.RaycastAll(agent.transform.position, (p - agent.transform.position).normalized);
            }

            return hits.Any(hit => tagFilter.tags.Contains(hit.transform.tag));
        }

        public override GUIContent GetConditionalContent()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("If raycast hits object");

            return new GUIContent(sb.ToString());
        }
    }
}