using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("Nodes/d_Quaternion"), LightIcon("Nodes/Quaternion"), Category("Quaternion"), Description(
         "Returns a rotation that rotates z degrees around the z axis, x degrees around the x axis, and y degrees around the y axis; applied in that order.")]
    public class EulerAngles : Action
    {
        [Tooltip("X Rotation")] public BlackboardEntrySelector<float> x;

        [Tooltip("Y Rotation")] public BlackboardEntrySelector<float> y;

        [Tooltip("Z Rotation")] public BlackboardEntrySelector<float> z;

        [Tooltip("Blackboard variable to store the new rotation in"), WriteOnly] 
        public BlackboardEntrySelector<Quaternion> rotation;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            rotation.value = Quaternion.Euler(x.value, y.value, z.value);

            return NodeStatus.Success;
        }
    }
}