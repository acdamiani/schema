using System;
using System.Collections.Generic;

namespace Schema.Utilities
{
    public static class NameAliases
    {
        public static readonly Dictionary<Type, string> aliases = new Dictionary<Type, string>
        {
            { typeof(byte), "byte" },
            { typeof(sbyte), "sbyte" },
            { typeof(short), "short" },
            { typeof(ushort), "ushort" },
            { typeof(int), "int" },
            { typeof(uint), "uint" },
            { typeof(long), "long" },
            { typeof(ulong), "ulong" },
            { typeof(float), "float" },
            { typeof(double), "double" },
            { typeof(decimal), "decimal" },
            { typeof(object), "object" },
            { typeof(bool), "bool" },
            { typeof(char), "char" },
            { typeof(string), "string" },
            { typeof(void), "void" }
        };

        public static string GetAliasForType(Type type)
        {
            if (aliases.ContainsKey(type))
                return aliases[type];

            return type.GetFriendlyTypeName();
        }
    }
}