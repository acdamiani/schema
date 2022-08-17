using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace Schema.Utilities
{
    public static class HelperMethods
    {
        private static readonly MD5 md5 = MD5.Create();
        private static Vector3[] toReturn = new Vector3[5];

        public static bool IsMac()
        {
#if UNITY_2017_1_OR_NEWER
            return SystemInfo.operatingSystemFamily == OperatingSystemFamily.MacOSX;
#else
            return SystemInfo.operatingSystem.StartsWith("Mac");
#endif
        }

        public static void MoveItemAtIndexToFront<T>(this T[] array, int index)
        {
            T item = array[index];
            for (int i = index; i > 0; i--)
                array[i] = array[i - 1];
            array[0] = item;
        }

        public static void MoveItemAtIndexToFront<T>(this List<T> list, int index)
        {
            T item = list[index];
            for (int i = index; i > 0; i--)
                list[i] = list[i - 1];
            list[0] = item;
        }

        public static T[] FilterArrayByMask<T>(T[] array, int mask)
        {
            T[] ret = new T[Mathf.Clamp(BitCount(mask), 0, array.Length)];

            if (ret.Length == 0)
                return ret;

            int j = 0;
            for (int i = array.Length - 1; i >= 0; i--)
            {
                bool isIncluded = (mask & (1 << (array.Length - i - 1))) != 0;

                if (isIncluded)
                {
                    ret[j] = array[i];
                    j++;
                }
            }

            return ret;
        }

        public static int BitCount(int u)
        {
            int count = 0;
            while (u != 0)
            {
                u = u & (u - 1);
                count++;
            }

            return count;
        }

        public static Vector3[] Circle(Vector2 center, float radius, int detail)
        {
            Vector3[] arr = new Vector3[detail];

            float turn = 360f / detail;
            float theta = 0;

            for (int i = 0; i < detail; i++)
            {
                float rad = theta * (Mathf.PI / 180f);

                float x = Mathf.Cos(rad) * radius;
                float y = Mathf.Sin(rad) * radius;

                arr[i] = new Vector2(x + center.x, y + center.y);

                theta += turn;
            }

            return arr;
        }

        public static void SetHideFlags(HideFlags hideFlags, params ScriptableObject[] objects)
        {
            foreach (ScriptableObject s in objects) s.hideFlags = hideFlags;
        }

        public static void SetHideFlags(HideFlags hideFlags, List<ScriptableObject> objects)
        {
            foreach (ScriptableObject s in objects) s.hideFlags = hideFlags;
        }

        public static void SetHideFlags(HideFlags hideFlags, params List<ScriptableObject>[] objects)
        {
            foreach (List<ScriptableObject> sl in objects)
                foreach (ScriptableObject s in sl)
                    s.hideFlags = hideFlags;
        }

        public static Color ToColor(this string s)
        {
            int i = s.GetHashCode();

            string hex = "#" +
                         ((i >> 16) & 0xFF).ToString("X2") +
                         ((i >> 8) & 0xFF).ToString("X2") +
                         (i & 0xFF).ToString("X2");

            ColorUtility.TryParseHtmlString(hex, out Color col);

            return col;
        }

        public static List<Node> GetAllParents(this Node node)
        {
            if (node.parent != null)
            {
                List<Node> ret = new();

                Node current = node;

                ret.Add(current);

                while (current.parent != null)
                {
                    ret.Add(current.parent);
                    current = current.parent;
                }

                return ret;
            }

            return new List<Node>();
        }

        public static IEnumerable<Type> GetNodeTypes()
        {
            //Gets all categories for Nodes
            return Assembly.GetAssembly(typeof(Node)).GetTypes().Where(type =>
                type != typeof(Root) && type.IsClass && type.BaseType == typeof(Node));
        }

        public static IEnumerable<Type> GetEnumerableOfType(Type type)
        {
            List<Type> objects = new();
            foreach (Type t in
                     Assembly.GetAssembly(type).GetTypes()
                         .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(type)))
                objects.Add(t);
            return objects;
        }

        public static bool ContentEqual(this GUIContent c1, GUIContent c2)
        {
            return c1.text == c2.text && c1.image == c2.image && c1.tooltip == c2.tooltip;
        }

        public static bool IsNumeric(this Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsDecimal(this Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }

        public static IList<T> Swap<T>(this IList<T> list, int indexA, int indexB)
        {
            if (indexA < 0 || indexA > list.Count - 1 || indexB < 0 || indexB > list.Count - 1)
                return list;

            (list[indexA], list[indexB]) = (list[indexB], list[indexA]);
            return list;
        }

        public static FieldInfo GetFieldFromPath(this Type type, string path)
        {
            List<string> pathParts = path.Split('.').ToList();
            FieldInfo current = null;

            while (pathParts.Count > 0)
            {
                string part = pathParts[0];

                current = (current == null ? type : current.FieldType).GetField(part,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                if (current == null)
                    return null;

                pathParts.RemoveAt(0);
            }

            return current;
        }

        public static Type GetMemoryType(this Type type)
        {
            return type
                .GetTypeInfo()
                .DeclaredNestedTypes
                .ElementAtOrDefault(0);
        }

        public static bool IsCastable(this Type from, Type to, bool implicitly = false)
        {
            return to.IsAssignableFrom(from) || from.HasCastDefined(to, implicitly);
        }

        private static bool HasCastDefined(this Type from, Type to, bool implicitly)
        {
            if ((from.IsPrimitive || from.IsEnum) && (to.IsPrimitive || to.IsEnum))
            {
                if (!implicitly)
                    return from == to || (from != typeof(bool) && to != typeof(bool));

                Type[][] typeHierarchy =
                {
                    new[] { typeof(byte), typeof(sbyte), typeof(char) },
                    new[] { typeof(short), typeof(ushort) },
                    new[] { typeof(int), typeof(uint) },
                    new[] { typeof(long), typeof(ulong) },
                    new[] { typeof(float) },
                    new[] { typeof(double) }
                };

                IEnumerable<Type> lowerTypes = Enumerable.Empty<Type>();
                foreach (Type[] types in typeHierarchy)
                {
                    if (types.Any(t => t == to))
                        return lowerTypes.Any(t => t == from);
                    lowerTypes = lowerTypes.Concat(types);
                }

                return false; // IntPtr, UIntPtr, Enum, Boolean
            }

            return IsCastDefined(to, m => m.GetParameters()[0].ParameterType, _ => from, implicitly, false)
                   || IsCastDefined(from, _ => to, m => m.ReturnType, implicitly, true);
        }

        private static bool IsCastDefined(Type type, Func<MethodInfo, Type> baseType,
            Func<MethodInfo, Type> derivedType, bool implicitly, bool lookInBase)
        {
            BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Static
                                                            | (lookInBase
                                                                ? BindingFlags.FlattenHierarchy
                                                                : BindingFlags.DeclaredOnly);
            return type.GetMethods(bindingFlags).Any(
                m => (m.Name == "op_Implicit" || (!implicitly && m.Name == "op_Explicit"))
                     && baseType(m).IsAssignableFrom(derivedType(m)));
        }

        public static string GetFriendlyTypeName(this Type type)
        {
            if (type.IsGenericParameter)
                return type.Name;

            if (!type.IsGenericType)
                return type.Name;

            StringBuilder builder = new();
            string name = type.Name;
            int index = name.IndexOf("`");
            builder.Append(name.Substring(0, index));
            builder.Append('<');
            bool first = true;
            foreach (Type arg in type.GetGenericArguments())
            {
                if (!first) builder.Append(',');
                builder.Append(GetFriendlyTypeName(arg));
                first = false;
            }

            builder.Append('>');
            return builder.ToString();
        }

        public static string Hash(this string str)
        {
            byte[] text = Encoding.UTF8.GetBytes(str);
            byte[] hash = md5.ComputeHash(text);

            StringBuilder builder = new();

            for (int i = 0; i < hash.Length; i++)
                builder.Append(hash[i].ToString("X2"));

            return builder.ToString();
        }

        public static Color Contrast(this Color color)
        {
            int d = 0;

            // Counting the perceptive luminance - human eye favors green color...      
            double luminance = (0.299 * color.r + 0.587 * color.g + 0.114 * color.b) / 255;

            if (luminance > 0.5)
                d = 0; // bright colors - black font
            else
                d = 255; // dark colors - white font

            return new Color(d, d, d);
        }

        public static Texture2D Tint(this Texture2D texture, Color color)
        {
            Texture2D ret = new(texture.width, texture.height);
            Color[] cols = new Color[texture.width * texture.height];

            for (int y = 0; y < texture.height; y++)
                for (int x = 0; x < texture.width; x++)
                    cols[y * texture.width + x] = texture.GetPixel(x, y) * color;

            ret.SetPixels(cols);
            ret.name = "Tinted";
            ret.Apply();

            return ret;
        }

        public static Rect Pad(this Rect rect, int padding)
        {
            return rect.Pad(new RectOffset(padding, padding, padding, padding));
        }

        public static Rect Pad(this Rect rect, RectOffset padding)
        {
            rect.x += padding.left;
            rect.y += padding.top;
            rect.width -= padding.left + padding.right;
            rect.height -= padding.top + padding.bottom;

            return rect;
        }

        public static Rect Scale(this Rect rect, Vector2 factor)
        {
            float w = rect.width * factor.x;
            float h = rect.height * factor.y;

            return new Rect(rect.x + (rect.width - w) / 2f, rect.y + (rect.height - h) / 2f, w, h);
        }

        public static Rect Scale(this Rect rect, float factor)
        {
            return rect.Scale(Vector2.one * factor);
        }

        public static Rect UseCenter(this Rect rect)
        {
            return new Rect(rect.x - rect.width / 2f, rect.y - rect.height / 2f, rect.width, rect.height);
        }

        public static Rect Slice(this Rect rect, float position, bool horizontal, bool forwards)
        {
            position = Mathf.Clamp01(position);

            if (horizontal)
            {
                position = position * rect.width + rect.x;

                return new Rect(forwards ? rect.x : position, rect.y, rect.xMax - position, rect.height);
            }

            position = position * rect.height + rect.y;

            return new Rect(rect.x, forwards ? rect.y : position, rect.width, rect.yMax - position);
        }

        public static Rect Normalize(this Rect rect)
        {
            rect.x = Mathf.Min(rect.x + rect.width, rect.x);
            rect.y = Mathf.Min(rect.y + rect.height, rect.y);
            rect.width = Mathf.Abs(rect.width);
            rect.height = Mathf.Abs(rect.height);

            return rect;
        }

    }
}