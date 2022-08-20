using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("d_Transform Icon", true), LightIcon("Transform Icon", true), Category("Vector"),
     Description("Get the angle between two vectors A and B")]
    public class VectorAngle : Action
    {
        public enum Dir
        {
            Up,
            Down,
            Left,
            Right,
            Forward,
            Backward
        }

        [Tooltip("Vector A")] public BlackboardEntrySelector vectorOne = new BlackboardEntrySelector();

        [Tooltip("Vector B")] public BlackboardEntrySelector vectorTwo = new BlackboardEntrySelector();

        [Tooltip("Whether to get the signed angle")]
        public bool signed;

        [Tooltip("Axis of rotation when evaluating the signed angle")]
        public BlackboardEntrySelector<Vector3> axis;

        [Tooltip("Direction to use for the axis of rotation")]
        public Dir direction;

        [Tooltip("Use a custom axis")] public bool overrideAxis;

        [Tooltip("Blackboard variable to store the angle between the vectors"), WriteOnly] 
        public BlackboardEntrySelector<float> angle;

        protected override void OnObjectEnable()
        {
            vectorOne.ApplyFilters(typeof(Vector2), typeof(Vector3));
            vectorTwo.ApplyFilters(typeof(Vector2), typeof(Vector3));

            ;
        }

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            if (signed)
            {
                if (vectorOne.entryType == typeof(Vector3) || vectorTwo.entryType == typeof(Vector3))
                {
                    Vector3 a = Vector3.zero;

                    if (overrideAxis)
                        a = axis.value;
                    else
                        switch (direction)
                        {
                            case Dir.Up:
                                a = Vector3.up;
                                break;
                            case Dir.Down:
                                a = Vector3.down;
                                break;
                            case Dir.Left:
                                a = Vector3.left;
                                break;
                            case Dir.Right:
                                a = Vector3.right;
                                break;
                            case Dir.Backward:
                                a = Vector3.back;
                                break;
                            case Dir.Forward:
                                a = Vector3.forward;
                                break;
                        }

                    angle.value = Vector3.SignedAngle((Vector3)vectorOne.value, (Vector3)vectorTwo.value, a);
                }
                else
                {
                    angle.value = Vector2.SignedAngle((Vector2)vectorOne.value, (Vector2)vectorTwo.value);
                }
            }
            else
            {
                angle.value = Vector3.Angle((Vector3)vectorOne.value, (Vector3)vectorTwo.value);
            }

            return NodeStatus.Success;
        }
    }
}