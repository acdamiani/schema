using System;
using System.Collections.Generic;
using Schema.Runtime;
using UnityEngine;

[DarkIcon("Dark/Selector")]
[LightIcon("Light/Selector")]
public class Selector : Flow
{
    public override int Tick(NodeStatus status, int index)
    {
        if (index + 1 > children.Count - 1 || status == NodeStatus.Success) return -1;

        return index + 1;
    }
}
