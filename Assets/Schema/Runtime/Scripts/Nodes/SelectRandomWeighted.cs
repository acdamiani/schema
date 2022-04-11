using UnityEngine;
using System.Runtime;
using Schema;
using System.Collections.Generic;
using System.Linq;

[DarkIcon("Dark/SelectRandomWeighted")]
[LightIcon("Light/SelectRandomWeighted")]
public class SelectRandomWeighted : Flow
{
    public SerializableDictionary<string, int> weights;
    public override void OnNodeEnter(object flowMemory, SchemaAgent agent)
    {
        foreach (Node child in children.Where(child => !weights.ContainsKey(child.uID)))
        {
            weights.Add(child.uID, 1);
        }
    }
    public override int Tick(NodeStatus status, int index)
    {
        if (index > -1) return -1;

        int ranWeight = Random.Range(1, weights.Values.Sum() + 1);

        foreach (Node child in children)
        {
            if (ranWeight <= weights[child.uID])
                return System.Array.IndexOf(children, child);

            ranWeight -= weights[child.uID];
        }
        return 0;
    }
}