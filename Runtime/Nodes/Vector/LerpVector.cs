using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("d_Transform Icon", true), LightIcon("Transform Icon", true), Category("Vector"),
     Description("Linearly interpolate between two Vectors")]
    public class LerpVector : Action
    {
        [Tooltip("Vector A")] public BlackboardEntrySelector vectorOne = new BlackboardEntrySelector();

        [Tooltip("Vector B")] public BlackboardEntrySelector vectorTwo = new BlackboardEntrySelector();

        [Tooltip("Amount to interpolate by")] public BlackboardEntrySelector<float> t =
            new BlackboardEntrySelector<float>();

        [Tooltip("Whether to clamp the t value")]
        public bool unclamped;

        [Tooltip("Blackboard variable to store the lerped vector in."), WriteOnly] 
        public BlackboardEntrySelector lerped = new BlackboardEntrySelector();

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
                    lerped.ApplyFilters(typeof(Vector2), typeof(Vector3), typeof(Vector4));
                    break;
                case 3:
                    lerped.ApplyFilters(typeof(Vector3), typeof(Vector4));
                    break;
                case 4:
                    lerped.ApplyFilters(typeof(Vector4));
                    break;
            }

            if (!unclamped)
                t.inspectorValue = Mathf.Clamp01(t.inspectorValue);
        }

        protected override void OnObjectEnable()
        {
            vectorOne.ApplyFilters(typeof(Vector2), typeof(Vector3), typeof(Vector4));
            vectorTwo.ApplyFilters(typeof(Vector2), typeof(Vector3), typeof(Vector4));

            ;
        }

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            if (unclamped)
                lerped.value = Vector4.LerpUnclamped((Vector4)vectorOne.value, (Vector4)vectorTwo.value, t.value);
            else
                lerped.value = Vector4.Lerp((Vector4)vectorOne.value, (Vector4)vectorTwo.value, t.value);

            return NodeStatus.Success;
        }
    }
}