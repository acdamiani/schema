using System;

namespace Schema
{
    [AttributeUsage(AttributeTargets.Field)]
    public class DisableDynamicBindingAttribute : Attribute
    {
    }
}