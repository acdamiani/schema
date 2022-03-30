using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Reflection;
using System.Linq;
using UnityEngine;

public static class DynamicPropertyBuilder
{
    public static void Build()
    {
        foreach (Type o in Blackboard.blackboardTypes)
        {
            Debug.Log(o);
        }
        Generate();
    }
    public static void Generate()
    {
        File.WriteAllText("Assets/Schema/Utilities/DynamicProperty.cs", GenerateClass(GenerateMethods()));
    }
    public static string GenerateMethods()
    {
        List<Type> types = Blackboard.blackboardTypes.ToList();

        string x = "";

        foreach (Type t in types)
        {
            List<Type> others = types.Where(x => x != t).ToList();

            string template = File.ReadAllText("Assets/Schema/Utilities/MethodTemplate.txt");
            string g = "";
            string s = "";
            template = template.Replace("{{type}}", t.Name);

            foreach (Type other in others)
            {
                string[] getters = PrintProperties(t, t, new List<Type> { other }, "", false).Select(x => x.ToString()).ToArray();
                string[] setters = PrintProperties(t, t, new List<Type> { other }, "", true).Select(x => x.ToString()).ToArray();

                int g_c = getters.Length;
                int s_c = setters.Length;

                for (int i = 0; i < g_c; i++)
                {
                    g += $"\t\t\tcase \"{getters[i]}\":\n\t\t\t\treturn obj{getters[i].Replace('/', '.')};\n";
                }

                for (int i = 0; i < s_c; i++)
                {
                    s += $"\t\t\tcase \"{setters[i]}\":\n\t\t\t\tobj{setters[i].Replace('/', '.')} = ({other.Name})value;\n\t\t\t\tbreak;{(i < s_c - 1 ? "\n" : "")}";
                }
            }

            g += "\t\t\tdefault:\n\t\t\t\treturn null;";

            template = template.Replace("{{get_switch}}", g);
            template = template.Replace("{{set_switch}}", s);

            x += template;
        }

        return x;
    }
    public static string GenerateClass(string methods)
    {
        string template = File.ReadAllText("Assets/Schema/Utilities/ClassTemplate.txt");
        template = template.Replace("{{methods}}", methods);

        string g = "";
        string s = "";

        foreach (Type t in Blackboard.blackboardTypes)
        {
            g += $"\t\t\tcase \"{t.AssemblyQualifiedName}\":\n\t\t\t\treturn GetPropertyFor{t.Name}(({t.Name})obj, path);\n";
            s += $"\t\t\tcase \"{t.AssemblyQualifiedName}\":\n\t\t\t\tSetPropertyFor{t.Name}(({t.Name})obj, path, value);\n\t\t\t\tbreak;\n";
        }

        g += "\t\t\tdefault:\n\t\t\t\treturn null;";

        template = template.Replace("{{get_switch}}", g);
        template = template.Replace("{{set_switch}}", s);
        template = template.Replace("{{methods}}", methods);

        return template;
    }
    private static IEnumerable<string> PrintProperties(Type baseType, Type type, List<Type> targets, string basePath, bool mustHaveSetter)
    {
        if (targets.Contains(type))
        {
            yield return basePath;
            yield break;
        }

        HashSet<Type> nonRecursiveTypes = new HashSet<Type> {
                typeof(sbyte),
                typeof(byte),
                typeof(short),
                typeof(ushort),
                typeof(int),
                typeof(uint),
                typeof(long),
                typeof(ulong),
                typeof(char),
                typeof(float),
                typeof(double),
                typeof(decimal),
                typeof(bool),
                typeof(string),
                typeof(Enum)
            };

        foreach (PropertyInfo field in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            ObsoleteAttribute obsoleteAttribute = field.GetCustomAttribute<ObsoleteAttribute>();

            if (obsoleteAttribute != null)
                continue;

            if (field.Name == "Item")
                continue;

            if (targets.Contains(field.PropertyType))
            {
                if (!mustHaveSetter || (field.SetMethod != null && !field.DeclaringType.IsValueType))
                    yield return basePath + "/" + field.Name;
            }
            else if (field.PropertyType != type && field.PropertyType != baseType && !nonRecursiveTypes.Any(t => t.IsAssignableFrom(field.PropertyType)))
            {
                foreach (string s in PrintProperties(baseType, field.PropertyType, targets, basePath + "/" + field.Name, mustHaveSetter))
                    if (!mustHaveSetter || (field.SetMethod != null && !field.DeclaringType.IsValueType))
                        yield return s;
            }
        }

        foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            ObsoleteAttribute obsoleteAttribute = field.GetCustomAttribute<ObsoleteAttribute>();

            if (obsoleteAttribute != null)
                continue;

            if (targets.Contains(field.FieldType))
            {
                yield return basePath + "/" + field.Name;
            }
            else if (field.FieldType != type && field.FieldType != baseType && !nonRecursiveTypes.Any(t => t.IsAssignableFrom(field.FieldType)))
            {
                foreach (string s in PrintProperties(baseType, field.FieldType, targets, basePath + "/" + field.Name, mustHaveSetter))
                    yield return s;
            }
        }
    }
}