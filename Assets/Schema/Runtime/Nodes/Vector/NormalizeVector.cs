using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("d_Transform Icon", true), LightIcon("Transform Icon", true), Description("Normalize a Vector"),
     Category("Vector")]
    public class NormalizeVector : Action
    {
        [Tooltip("Vector to normalize")] public BlackboardEntrySelector vector = new BlackboardEntrySelector();

        [Tooltip("Blackboard variable to store the normalized vector in"), WriteOnly] 
        public BlackboardEntrySelector normalized = new BlackboardEntrySelector();

        private void OnValidate()
        {
            switch (vector.entryType?.Name)
            {
                case "Vector2":
                    normalized.ApplyFilter<Vector2>();
                    break;
                case "Vector3":
                    normalized.ApplyFilter<Vector3>();
                    break;
                case "Vector4":
                    normalized.ApplyFilter<Vector4>();
                    break;
            }
        }

        protected override void OnObjectEnable()
        {
            vector.ApplyFilters(typeof(Vector2), typeof(Vector3), typeof(Vector4));

            ;
        }

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            normalized.value = Vector4.Normalize((Vector4)vector.value);

            return NodeStatus.Success;
        }
    }
}