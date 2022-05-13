using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Schema;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("c_Transform")]
    [LightIcon("c_Transform")]
    [Description("Take the cross product of two Vector3 values")]
    [Category("Vector")]
    public class Cross : Action
    {
        [Tooltip("LHS of the cross product")]
        public BlackboardEntrySelector<Vector3> vectorOne;
        [Tooltip("RHS of the cross product")]
        public BlackboardEntrySelector<Vector3> vectorTwo;
        [Tooltip("Blackboard variable to store the cross product in"), WriteOnly]
        public BlackboardEntrySelector<Vector3> cross;
        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            Vector3 product = Vector3.Cross(vectorOne.value, vectorTwo.value);

            cross.value = product;

            return NodeStatus.Success;
        }
    }
}