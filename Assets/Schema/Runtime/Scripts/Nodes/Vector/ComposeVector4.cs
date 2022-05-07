using UnityEngine;
using Schema;

namespace Schema.Builtin.Nodes
{
    [Description("Composes a Vector4 with given x, y, z, and w values")]
    public class ComposeVector4 : Action
    {
        public BlackboardEntrySelector<float> x;
        public BlackboardEntrySelector<float> y;
        public BlackboardEntrySelector<float> z;
        public BlackboardEntrySelector<float> w;
        [Tooltip("Entry to store composed vector in"), WriteOnly] public BlackboardEntrySelector<Vector4> target;
        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            target.value = new Vector4(x.value, y.value, z.value, w.value);

            return NodeStatus.Success;
        }
    }
}