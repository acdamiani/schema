using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("d_Transform Icon", true), LightIcon("Transform Icon", true),
     Description("Composes a Vector3 with given x, y and z values"), Category("Vector")]
    public class ComposeVector3 : Action
    {
        public BlackboardEntrySelector<float> x;
        public BlackboardEntrySelector<float> y;
        public BlackboardEntrySelector<float> z;

        [Tooltip("Entry to store composed vector in"), WriteOnly] 
        public BlackboardEntrySelector<Vector3> target;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            target.value = new Vector3(x.value, y.value, z.value);

            return NodeStatus.Success;
        }
    }
}