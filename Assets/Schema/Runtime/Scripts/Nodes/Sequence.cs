using System;
using System.Collections.Generic;
using Schema.Runtime;
using UnityEngine;

[DarkIcon("Dark/Sequence")]
[LightIcon("Light/Sequence")]
public class Sequence : Flow
{
    public override int Tick(NodeStatus status, int index)
    {
        if (index + 1 > children.Count - 1 || status == NodeStatus.Failure) return -1;
        else return index + 1;
    }
}