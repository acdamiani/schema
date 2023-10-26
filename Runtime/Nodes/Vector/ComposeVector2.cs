using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("d_Transform Icon", true), LightIcon("Transform Icon", true), Category("Vector"),
     Description("Composes a Vector2 with given x and y values")]
    public class ComposeVector2 : Action
    {
        public BlackboardEntrySelector<float> x;
        public BlackboardEntrySelector<float> y;

        [Tooltip("Entry to store composed vector in"), WriteOnly] 
        public BlackboardEntrySelector<Vector2> target;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            target.value = new Vector2(x.value, y.value);

            return NodeStatus.Success;
        }
    }
}