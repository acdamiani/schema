using UnityEngine;
using Schema.Runtime;

[DarkIcon("Dark/SelectRandom")]
[LightIcon("Light/SelectRandom")]
public class SelectRandom : Flow
{
    public override int Tick(NodeStatus status, int index)
    {
        if (index > -1) return -1;

        return Random.Range(0, children.Count);
    }
}