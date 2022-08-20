using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("Nodes/d_Math"), LightIcon("Nodes/Math"), Category("Math"),
     Description("Get the greatest integer less than or equal to a float")]
    public class Floor : Action
    {
        [Tooltip("Float to floor")] public BlackboardEntrySelector<float> value;

        [Tooltip("Selector to store the floored value in"), WriteOnly] 
        public BlackboardEntrySelector<float> result;

        [Tooltip("Convert the floored value to an integer")]
        public bool floorToInt = true;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            result.value = Mathf.Floor(value.value);

            return NodeStatus.Success;
        }
    }
}