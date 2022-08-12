using System;
using System.Linq;
using UnityEngine;

public class RequireAgentComponentAttribute : Attribute
{
    public readonly Type[] types;

    public RequireAgentComponentAttribute(params Type[] types)
    {
        if (types != null)
            this.types = types.Where(t1 => typeof(Component).IsAssignableFrom(t1)).ToArray();
    }
}