using System;
using System.Collections.Generic;

[Serializable]
public class NavMeshAreaMask
{
    public List<string> areas = new List<string>();
    public int mask = -1;
}