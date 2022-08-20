using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("Nodes/d_Selector"), LightIcon("Nodes/Selector")]
    public class SelectRandom : Flow
    {
        public override int Tick(object nodeMemory, NodeStatus status, int index)
        {
            if (index > -1) return -1;

            return Random.Range(0, children.Length);
        }
    }
}