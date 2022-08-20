using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("d_Transform Icon", true), LightIcon("Transform Icon", true),
     Description("Decompose a vector to get its components"), Category("Vector")]
    public class GetVectorComponents : Action
    {
        [Tooltip("Vector to use to get components")]
        public BlackboardEntrySelector vector = new BlackboardEntrySelector();

        [WriteOnly] public BlackboardEntrySelector<float> x;
        [WriteOnly] public BlackboardEntrySelector<float> y;
        [WriteOnly] public BlackboardEntrySelector<float> z;
        [WriteOnly] public BlackboardEntrySelector<float> w;

        protected override void OnObjectEnable()
        {
            vector.ApplyFilters(typeof(Vector2), typeof(Vector3), typeof(Vector4));
        }

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            if (vector.entryType == typeof(Vector2))
            {
                Vector2 v = (Vector2)vector.value;

                x.value = v.x;
                y.value = v.y;
            }
            else if (vector.entryType == typeof(Vector3))
            {
                Vector3 v = (Vector3)vector.value;

                x.value = v.x;
                y.value = v.y;
                z.value = v.z;
            }
            else if (vector.entryType == typeof(Vector4))
            {
                Vector4 v = (Vector4)vector.value;

                x.value = v.x;
                y.value = v.y;
                z.value = v.z;
                w.value = v.w;
            }

            return NodeStatus.Success;
        }
    }
}