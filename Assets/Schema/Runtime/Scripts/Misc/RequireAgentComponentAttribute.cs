using System;
using System.Collections.Generic;
using System.Linq;

public class RequireAgentComponentAttribute : Attribute
{
    public readonly Type[] types;
    public RequireAgentComponentAttribute(params Type[] types)
    {
        if (types != null)
            this.types = types.Where(t1 => typeof(UnityEngine.Component).IsAssignableFrom(t1)).ToArray();
    }
}