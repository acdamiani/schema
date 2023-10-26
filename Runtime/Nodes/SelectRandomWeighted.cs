using System;
using System.Linq;
using Random = UnityEngine.Random;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("Nodes/d_Selector"), LightIcon("Nodes/Selector")]
    public class SelectRandomWeighted : Flow
    {
        public SerializableDictionary<string, int> weights;

        public override void OnFlowEnter(object flowMemory, SchemaAgent agent)
        {
            foreach (Node child in children.Where(child => !weights.ContainsKey(child.uID))) weights.Add(child.uID, 1);
        }

        public override int Tick(object nodeMemory, NodeStatus status, int index)
        {
            if (index > -1) return -1;

            int ranWeight = Random.Range(1, weights.Values.Sum() + 1);

            foreach (Node child in children)
            {
                if (ranWeight <= weights[child.uID])
                    return Array.IndexOf(children, child);

                ranWeight -= weights[child.uID];
            }

            return 0;
        }
    }
}