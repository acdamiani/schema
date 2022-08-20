using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("Nodes/d_Quaternion"), LightIcon("Nodes/Quaternion"), Category("Quaternion"),
     Description("Creates a rotation which rotates a specified number of degrees around an axis")]
    public class QuaternionAngleAxis : Action
    {
        [Tooltip("Angle (in degrees) to rotate around an axis")]
        public BlackboardEntrySelector<float> angle;

        [Tooltip("Use a custom axis for rotation")]
        public bool overrideAxis;

        [Tooltip("Axis to rotate around")] public VectorAngle.Dir direction;

        [Tooltip("Custom axis to rotate around")]
        public BlackboardEntrySelector<Vector3> axis;

        [Tooltip("Blackboard variable to store the new rotation in"), WriteOnly] 
        public BlackboardEntrySelector<Quaternion> rotation;

        private void OnValidate()
        {
            angle.inspectorValue = angle.inspectorValue % 360f;
        }

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            Vector3 axis = Vector3.zero;

            if (overrideAxis)
                axis = this.axis.value;
            else
                switch (direction)
                {
                    case VectorAngle.Dir.Up:
                        axis = Vector3.up;
                        break;
                    case VectorAngle.Dir.Down:
                        axis = Vector3.down;
                        break;
                    case VectorAngle.Dir.Left:
                        axis = Vector3.left;
                        break;
                    case VectorAngle.Dir.Right:
                        axis = Vector3.right;
                        break;
                    case VectorAngle.Dir.Forward:
                        axis = Vector3.forward;
                        break;
                    case VectorAngle.Dir.Backward:
                        axis = Vector3.back;
                        break;
                }

            rotation.value = Quaternion.AngleAxis(angle.value, axis);

            return NodeStatus.Success;
        }
    }
}