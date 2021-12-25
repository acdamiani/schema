using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Schema.Runtime;

namespace Schema.Utilities
{
    public static class HelperMethods
    {
        public static bool IsMac()
        {
#if UNITY_2017_1_OR_NEWER
            return SystemInfo.operatingSystemFamily == OperatingSystemFamily.MacOSX;
#else
            return SystemInfo.operatingSystem.StartsWith("Mac");
#endif
        }
        static Vector3[] toReturn = new Vector3[5];
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
        public static Texture2D Invert(this Texture2D texture)
        {
            if (!texture.isReadable)
            {
                Debug.LogWarning(
                        "When importing custom icons for nodes, be sure to import them with the \"Read/Write Enabled\" checkbox checked. Otherwise, colors will not be modified for light/dark mode"
                        );
                return texture;
            }

            Texture2D tex = new Texture2D(texture.width, texture.height);

            for (int y = 0; y < texture.height; y++)
            {
                for (int x = 0; x < texture.width; x++)
                {
                    Color pixel = texture.GetPixel(x, y);

                    tex.SetPixel(x, y, new Color(1f - pixel.r, 1f - pixel.g, 1f - pixel.b, pixel.a));
                }
            }

            tex.Apply();
            return tex;
        }
        public static void Move<T>(this List<T> list, T item, int newIndex)
        {
            if (item != null)
            {
                var oldIndex = list.IndexOf(item);
                if (oldIndex > -1)
                {
                    list.RemoveAt(oldIndex);

                    if (newIndex > oldIndex) newIndex--;
                    // the actual index could have shifted due to the removal

                    list.Insert(newIndex, item);
                }
            }

        }
        public static Vector3[] Circle(Vector2 center, float radius, int detail)
        {
            Vector3[] arr = new Vector3[detail];

            float turn = 360f / (float)detail;
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
            foreach (ScriptableObject s in objects)
            {
                s.hideFlags = hideFlags;
            }
        }
        public static void SetHideFlags(HideFlags hideFlags, List<ScriptableObject> objects)
        {
            foreach (ScriptableObject s in objects)
            {
                s.hideFlags = hideFlags;
            }
        }
        public static void SetHideFlags(HideFlags hideFlags, params List<ScriptableObject>[] objects)
        {
            foreach (List<ScriptableObject> sl in objects)
            {
                foreach (ScriptableObject s in sl)
                {
                    s.hideFlags = hideFlags;
                }
            }
        }
        public static Color ToColor(this string s)
        {
            int i = s.GetHashCode();

            string hex = "#" +
                (((i >> 16) & 0xFF)).ToString("X2") +
                (((i >> 8) & 0xFF)).ToString("X2") +
                ((i & 0xFF)).ToString("X2");

            ColorUtility.TryParseHtmlString(hex, out Color col);

            return col;
        }
        public static List<Node> GetAllParents(this Node node)
        {
            if (node.parent != null)
            {
                List<Node> ret = new List<Node>();

                Node current = node;

                ret.Add(current);

                while (current.parent != null)
                {
                    ret.Add(current.parent);
                    current = current.parent;
                }

                return ret;
            }
            else
            {
                return new List<Node>();
            }
        }
        public static IEnumerable<Type> GetNodeTypes()
        {
            //Gets all categories for Nodes
            return Assembly.GetAssembly(typeof(Node)).GetTypes().Where(type => type != typeof(Root) && type.IsClass && type.BaseType == typeof(Node));
        }
        public static IEnumerable<Type> GetEnumerableOfType(Type type, params object[] constructorArgs)
        {
            List<Type> objects = new List<Type>();
            foreach (Type t in
                Assembly.GetAssembly(type).GetTypes()
                .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(type)))
            {
                objects.Add(t);
            }
            return objects;
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
    }
}
