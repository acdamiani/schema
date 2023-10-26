using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("d_Transform Icon", true), LightIcon("Transform Icon", true),
     Description("Composes a Vector4 with given x, y, z, and w values"), Category("Vector")]
    public class ComposeVector4 : Action
    {
        public BlackboardEntrySelector<float> x;
        public BlackboardEntrySelector<float> y;
        public BlackboardEntrySelector<float> z;
        public BlackboardEntrySelector<float> w;

        [Tooltip("Entry to store composed vector in"), WriteOnly] 
        public BlackboardEntrySelector<Vector4> target;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            target.value = new Vector4(x.value, y.value, z.value, w.value);

            return NodeStatus.Success;
        }
    }
}