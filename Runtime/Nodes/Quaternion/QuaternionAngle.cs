using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("Nodes/d_Quaternion"), LightIcon("Nodes/Quaternion"), Category("Quaternion"),
     Description("Get the angle between two quaternion rotations")]
    public class QuaternionAngle : Action
    {
        [Tooltip("Quaternion A")] public BlackboardEntrySelector<Quaternion> quaternionOne;

        [Tooltip("Quaternion B")] public BlackboardEntrySelector<Quaternion> quaternionTwo;

        [Tooltip("Blackboard variable to store the angle in"), WriteOnly] 
        public BlackboardEntrySelector<float> angle;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            angle.value = Quaternion.Angle(quaternionOne.value, quaternionTwo.value);

            return NodeStatus.Success;
        }
    }
}