using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("Nodes/d_CoinFlip"), LightIcon("Nodes/CoinFlip"), Category("Random")]
    public class CoinFlip : Action
    {
        [WriteOnly] public BlackboardEntrySelector<bool> entry;

        [Tooltip("Chance that the entry will be true"), Range(0, 1)] 
        public float chance;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            bool v = Random.Range(0f, 1f) <= chance;
            entry.value = v;

            return NodeStatus.Success;
        }
    }
}