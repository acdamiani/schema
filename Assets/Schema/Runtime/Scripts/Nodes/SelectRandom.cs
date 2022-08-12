using Schema;
using UnityEngine;

[DarkIcon("Dark/SelectRandom")]
[LightIcon("Light/SelectRandom")]
public class SelectRandom : Flow
{
    public override int Tick(object nodeMemory, NodeStatus status, int index)
    {
        if (index > -1) return -1;

        return Random.Range(0, children.Length);
    }
}