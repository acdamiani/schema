using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("d_Transform Icon", true), LightIcon("Transform Icon", true), Description(
         "Calculate a position between the points specified by current and target, moving no farther than the distance specified by maxDistanceDelta."),
     Category("Vector")]
    public class MoveTowardsVector : Action
    {
        [Tooltip("The position to move from.")]
        public BlackboardEntrySelector vectorOne = new BlackboardEntrySelector();

        [Tooltip("The position to move towards.")]
        public BlackboardEntrySelector vectorTwo = new BlackboardEntrySelector();

        [Tooltip("Distance to move current per call.")]
        public BlackboardEntrySelector<float> maxDistanceDelta;

        [Tooltip("Blackboard variable to store the new position vector in"), WriteOnly] 
        public BlackboardEntrySelector newPosition = new BlackboardEntrySelector();

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
                    newPosition.ApplyFilters(typeof(Vector2), typeof(Vector3), typeof(Vector4));
                    break;
                case 3:
                    newPosition.ApplyFilters(typeof(Vector3), typeof(Vector4));
                    break;
                case 4:
                    newPosition.ApplyFilters(typeof(Vector4));
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
            newPosition.value =
                Vector4.MoveTowards((Vector4)vectorOne.value, (Vector4)vectorTwo.value, maxDistanceDelta.value);

            return NodeStatus.Success;
        }
    }
}