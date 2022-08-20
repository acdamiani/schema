using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("d_Transform Icon", true), LightIcon("Transform Icon", true), Category("Vector"),
     Description("Projects a vector onto another vector")]
    public class ProjectVector : Action
    {
        [Tooltip("Vector A")] public BlackboardEntrySelector vectorOne = new BlackboardEntrySelector();

        [Tooltip("Vector B")] public BlackboardEntrySelector vectorTwo = new BlackboardEntrySelector();

        [Tooltip("Blackboard variable to store the new projected vector in"), WriteOnly] 
        public BlackboardEntrySelector projected = new BlackboardEntrySelector();

        private void OnValidate()
        {
            int l = 0;

            switch (vectorOne.entryType?.Name)
            {
                case "Vector2":
                    l = Mathf.Max(l, 2);
                    break;
                case "Vector3":
                    l = Mathf.Max(l, 3);
                    break;
                case "Vector4":
                    l = Mathf.Max(l, 4);
                    break;
            }

            switch (vectorTwo.entryType?.Name)
            {
                case "Vector2":
                    l = Mathf.Max(l, 2);
                    break;
                case "Vector3":
                    l = Mathf.Max(l, 3);
                    break;
                case "Vector4":
                    l = Mathf.Max(l, 4);
                    break;
            }

            switch (l)
            {
                case 2:
                    projected.ApplyFilters(typeof(Vector2), typeof(Vector3), typeof(Vector4));
                    break;
                case 3:
                    projected.ApplyFilters(typeof(Vector3), typeof(Vector4));
                    break;
                case 4:
                    projected.ApplyFilters(typeof(Vector4));
                    break;
            }
        }

        protected override void OnObjectEnable()
        {
            vectorOne.ApplyFilters(typeof(Vector2), typeof(Vector3), typeof(Vector4));
            vectorTwo.ApplyFilters(typeof(Vector2), typeof(Vector3), typeof(Vector4));

            ;
        }

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            projected.value = Vector4.Project((Vector4)vectorOne.value, (Vector4)vectorTwo.value);

            return NodeStatus.Success;
        }
    }
}