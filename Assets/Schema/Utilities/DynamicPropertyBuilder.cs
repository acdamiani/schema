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
        // foreach (Type o in Blackboard.blackboardTypes)
        // {
        //     Debug.Log(o);
        // }
        Generate();

        // Debug.Log(GenerateSetMethod(typeof(GameObject), "test", "transform.position", 0));
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
                string[] getters = PrintProperties(t, t, new List<Type> { other }, "", false, false).Select(x => x.ToString()).ToArray();
                string[] setters = PrintProperties(t, t, new List<Type> { other }, "", true, false).Select(x => x.ToString()).ToArray();

                int g_c = getters.Length;
                int s_c = setters.Length;

                for (int i = 0; i < g_c; i++)
                {
                    g += $"\t\t\tcase \"{getters[i]}\":\n\t\t\t\treturn obj{getters[i].Replace('/', '.')};\n";
                }

                for (int i = 0; i < s_c; i++)
                {
                    s += GenerateSetMethod(t, "obj", setters[i], i);
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
    private struct UnrolledStruct
    {
        public Type parentType;
        public Type type;
        public string pathPart;
        public string targetPathPart;
    }
    private static string GenerateSetMethod(Type type, string variable, string path, int idOffset)
    {
        List<UnrolledStruct> needsUnrolling = new List<UnrolledStruct>();

        string[] paths = path.Trim('.', '/').Split('.', '/');
        string incompletePath = ".";

        Type cur = type;
        Type root = null;

        string s = $"case \"{path}\":\n{{";

        if (paths.Length == 1)
        {
            FieldInfo field = cur.GetField(paths[0]);
            PropertyInfo property = cur.GetProperty(paths[0]);

            s += $"{variable}.{path.Trim('.', '/').Replace('/', '.')} = ({(field != null ? field.FieldType : property.PropertyType)})value;}}\nbreak;\n";

            return s;
        }

        for (int i = 0; i < paths.Length; i++)
        {
            string chunk = paths[i];
            incompletePath += chunk;

            FieldInfo field = cur.GetField(chunk);

            if (field != null)
            {
                cur = field.FieldType;
                root = cur;
                continue;
            }

            PropertyInfo property = cur.GetProperty(chunk);

            if (property == null)
            {
                Debug.Log("This is why2");
                return "";
            }

            if (property.PropertyType.IsValueType && property.SetMethod != null && i < paths.Length - 1)
            {
                UnrolledStruct unrolled = new UnrolledStruct();
                unrolled.parentType = property.DeclaringType;
                unrolled.type = property.PropertyType;
                unrolled.pathPart = incompletePath;
                unrolled.targetPathPart = i + 1 <= paths.Length - 1 ? paths[i + 1] : "";

                needsUnrolling.Add(unrolled);
            }
            else if (property.PropertyType.IsValueType && property.SetMethod == null)
            {
                Debug.Log("this is why: " + property.Name);
                return "";
            }
            cur = property.PropertyType;
            root = property.PropertyType;

            incompletePath += '.';
        }

        for (int i = 0; i < needsUnrolling.Count; i++)
        {
            UnrolledStruct t = needsUnrolling[i];

            s += $"{t.type} {IDFromInt(i + idOffset)} = {variable}{t.pathPart};\n";
        }

        for (int i = needsUnrolling.Count - 1; i >= 0; i--)
        {
            UnrolledStruct t = needsUnrolling[i];

            s += $"{IDFromInt(i + idOffset)}.{t.targetPathPart} = {(i == needsUnrolling.Count - 1 ? $"({root})value" : IDFromInt(i + idOffset + 1))};\n";
        }

        incompletePath = incompletePath.TrimEnd('.');

        if (needsUnrolling.Count > 0)
            s += $"{variable}{needsUnrolling[0].pathPart} = {IDFromInt(idOffset)};\n";
        else
            s += $"{variable}{incompletePath} = ({root})value;\n";

        s += "}\nbreak;\n";

        return s;
    }
    private static string IDFromInt(int count)
    {
        if (count < 0)
            return "";

        if (count == 0)
            return "a";

        string result = "";
        int i = 1;
        int o = count;

        while (count >= 0)
        {
            result = ((char)((int)'a' + count % 26)) + result;

            count = o / IPow(26, i);
            count--;

            i++;
        }

        return result;
    }
    private static int IPow(int baseInt, int exp)
    {
        int result = 1;
        while (exp > 0)
        {
            if ((exp & 1) != 0)
                result *= baseInt;
            exp >>= 1;
            baseInt *= baseInt;
        }
        return result;
    }
    private static IEnumerable<string> PrintProperties(Type baseType, Type type, List<Type> targets, string basePath, bool needsGetter, bool useType)
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
                if (!needsGetter || field.SetMethod != null)
                    yield return basePath + "/" + field.Name + (useType ? " (" + field.PropertyType.Name + ")" : "");
            }
            else if (field.PropertyType != type && field.PropertyType != baseType && !nonRecursiveTypes.Any(t => t.IsAssignableFrom(field.PropertyType)))
            {
                foreach (string s in PrintProperties(baseType, field.PropertyType, targets, basePath + "/" + field.Name, needsGetter, useType))
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
                yield return basePath + "/" + field.Name + (useType ? " " + field.FieldType : "");
            }
            else if (field.FieldType != type && field.FieldType != baseType && !nonRecursiveTypes.Any(t => t.IsAssignableFrom(field.FieldType)))
            {
                foreach (string s in PrintProperties(baseType, field.FieldType, targets, basePath + "/" + field.Name, needsGetter, useType))
                    yield return s;
            }
        }
    }
}