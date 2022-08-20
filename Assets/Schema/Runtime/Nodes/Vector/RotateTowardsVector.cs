using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("d_Transform Icon", true), LightIcon("Transform Icon", true),
     Description("Rotates a vector towards a target"), Category("Vector")]
    public class RotateTowardsVector : Action
    {
        [Tooltip("Current managed vector")] public BlackboardEntrySelector current = new BlackboardEntrySelector();

        [Tooltip("Target vector")] public BlackboardEntrySelector target = new BlackboardEntrySelector();

        [Tooltip("The maximum angle in radians allowed for this rotation")]
        public BlackboardEntrySelector<float> maxRadiansDelta;

        [Tooltip("The maximum allowed change in vector magnitude for this rotation")]
        public BlackboardEntrySelector<float> maxMagnitudeDelta;

        [Tooltip("Blackboard variable to store the new rotated vector in"), WriteOnly] 
        public BlackboardEntrySelector rotated = new BlackboardEntrySelector();

        private void OnValidate()
        {
            int l = 0;

            switch (current.entryType?.Name)
            {
                case "Vector2":
                    l = Mathf.Max(l, 2);
                    break;
                case "Vector3":
                    l = Mathf.Max(l, 3);
                    break;
            }

            switch (target.entryType?.Name)
            {
                case "Vector2":
                    l = Mathf.Max(l, 2);
                    break;
                case "Vector3":
                    l = Mathf.Max(l, 3);
                    break;
            }

            switch (l)
            {
                case 2:
                    rotated.ApplyFilters(typeof(Vector2), typeof(Vector3));
                    break;
                case 3:
                    rotated.ApplyFilters(typeof(Vector3));
                    break;
            }
        }

        protected override void OnObjectEnable()
        {
            current.ApplyFilters(typeof(Vector2), typeof(Vector3));
            target.ApplyFilters(typeof(Vector2), typeof(Vector3));
            rotated.ApplyFilters(typeof(Vector2), typeof(Vector3));

            ;
        }

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            rotated.value = Vector3.RotateTowards((Vector3)current.value, (Vector3)target.value, maxRadiansDelta.value,
                maxMagnitudeDelta.value);

            return NodeStatus.Success;
        }
    }
}