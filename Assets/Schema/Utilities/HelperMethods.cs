using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEngine;
using UnityEditor;
using Schema;

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
        public static void Move<T>(this T[] array, T item, int newIndex)
        {
            if (item != null)
            {
                int oldIndex = Array.IndexOf(array, item);

                if (oldIndex > -1)
                {
                    ArrayUtility.RemoveAt(ref array, oldIndex);

                    if (newIndex > oldIndex) newIndex--;

                    ArrayUtility.Insert(ref array, newIndex, item);
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
        public static FieldInfo GetFieldFromPath(this Type type, string path)
        {
            List<string> pathParts = path.Split('.').ToList();
            FieldInfo current = null;

            while (pathParts.Count > 0)
            {
                string part = pathParts[0];

                current = (current == null ? type : current.FieldType).GetField(part, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                if (current == null)
                    return null;

                pathParts.RemoveAt(0);
            }

            return current;
        }
        public static Type FindType(string fullName)
        {
            return
                AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => !a.IsDynamic)
                    .SelectMany(a => a.GetTypes())
                    .FirstOrDefault(t => t.FullName.Equals(fullName));
        }
    }
}
