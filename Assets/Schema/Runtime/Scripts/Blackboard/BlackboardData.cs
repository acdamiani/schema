using System;
using System.Collections.Generic;
using System.Reflection;

internal static class BlackboardDataContainer
{
    public static void Initialize(Blackboard blackboard)
    {
        foreach (BlackboardEntry entry in blackboard.entries)
        {
            if (!values.ContainsKey(entry.uID))
                values.Add(entry.uID, new EntryData(entry));
        }
    }
    private static Dictionary<string, EntryData> values = new Dictionary<string, EntryData>();
    public static object Get(string id, int pid)
    {
        values.TryGetValue(id, out EntryData data);

        return data?.GetValue(pid);
    }
    public static void Set(string id, int pid, object value)
    {
        values.TryGetValue(id, out EntryData data);

        data?.SetValue(pid, value);
    }
    internal class EntryData
    {
        public List<object> value = new List<object>();
        private object defaultValue;
        public BlackboardEntry.EntryType type;
        public EntryData(BlackboardEntry entry)
        {
            type = entry.entryType;
            defaultValue = entry.type.IsValueType ? Activator.CreateInstance(entry.type) : null;
            value.Add(defaultValue);
        }
        public object GetValue(int pid)
        {
            if (type == BlackboardEntry.EntryType.Local)
            {
                if (pid > value.Count - 1)
                {
                    while (pid > value.Count - 1)
                    {
                        value.Add(defaultValue);
                    }
                    return defaultValue;
                }

                return value[pid];
            }
            else
            {
                return value[0];
            }
        }
        public void SetValue(int pid, object v)
        {
            if (type == BlackboardEntry.EntryType.Local)
            {
                if (pid > value.Count - 1)
                {
                    while (pid > value.Count - 2)
                    {
                        value.Add(defaultValue);
                    }

                    value.Add(v);

                    return;
                }

                value[pid] = v;
            }
            else
            {
                value[0] = v;
            }
        }
    }
}
// using System;
// using System.Collections.Generic;
// using UnityEngine;
// using Schema;

// public class BlackboardData
// {
//     public T GetValueReflected<T>(BlackboardEntrySelector selector, string path)
//     {
//         string[] pathParts = path.Split('/');

//         return default(T);
//     }
//     public T GetValueForSelector<T>(BlackboardEntrySelector<T> selector, string path)
//     {
//         return null;
//     }
//     public object GetEntryValue(string id, string pid)
//     {
//         EntryData data;
//     }
//     private Dictionary<string, EntryData> dictValues = new Dictionary<string, EntryData>();
//     private Dictionary<string, EntryData> locals = new Dictionary<string, EntryData>();
//     private Dictionary<string, EntryData> shared = new Dictionary<string, EntryData>();
//     private Dictionary<string, EntryData> globals = new Dictionary<string, EntryData>();
//     public struct EntryData
//     {
//         public BlackboardEntry.EntryType entryType;
//         public object value;
//         public EntryData(BlackboardEntry.EntryType entryType, object value)
//         {
//             this.value = value;
//             this.entryType = entryType;
//         }
//     }
//     private Dictionary<string, EntryData> values = new Dictionary<string, EntryData>();
//     public BlackboardData(Blackboard blackboard)
//     {
//         Initialize(blackboard);
//     }
//     private void Initialize(Blackboard blackboard)
//     {
//         foreach (BlackboardEntry entry in blackboard.entries)
//         {
//             object defaultValue = GetDefault(Type.GetType(entry.typeString));

//             values.Add(entry.uID, new EntryData(entry.name, defaultValue, Type.GetType(entry.typeString)));
//         }
//     }
//     private object GetDefault(Type t)
//     {
//         if (t.IsValueType)
//         {
//             return Activator.CreateInstance(t);
//         }
//         return null;
//     }
//     public object GetValue(BlackboardEntrySelector selector)
//     {
//         return GetValue(selector.entryID);
//     }
//     public Type GetType(BlackboardEntrySelector selector)
//     {
//         return GetValue(selector).GetType();
//     }
//     public object GetValue(string id)
//     {
//         if (string.IsNullOrEmpty(id))
//         {
//             return null;
//         }

//         if (values.ContainsKey(id))
//         {
//             return values[id].value;
//         }
//         else
//         {
//             return null;
//         }
//     }
//     public T GetValue<T>(BlackboardEntrySelector selector)
//     {
//         if (selector == null || String.IsNullOrEmpty(selector.entryID)) return default(T);

//         return GetValue<T>(selector.entryID);
//     }
//     public T GetValue<T>(string id)
//     {
//         if (values.ContainsKey(id))
//         {
//             return (T)(values[id].value);
//         }
//         else
//         {
//             return default(T);
//         }
//     }
//     public void SetValue<T>(BlackboardEntrySelector selector, T value)
//     {
//         SetValue<T>(selector.entryID, value);
//     }
//     public void SetValue<T>(string id, T value)
//     {
//         EntryData v = values[id];
//         if (value == null)
//         {
//             values[id] = new EntryData(v.name, null, v.type);
//             return;
//         }

//         if (value.GetType() == typeof(T))
//         {
//             values[id] = new EntryData(v.name, value, v.type);
//         }
//         else
//         {
//             throw new UnityException($"Cannot set the value of blackboard key {id} because the types are not equivalent.");
//         }
//     }
//     /// <summary>
//     /// Gets the Type of the entry specified by the provided id
//     /// </summary>
//     /// <param name="id">The id of the entry to retrieve</param>
//     /// <returns>The Type of the entry</returns>
//     public Type GetEntryType(string id)
//     {
//         return values[id].type;
//     }
//     /// <summary>
//     /// Get the EntryData for the entry with the specified id
//     /// </summary>
//     /// <param name="id">The id of the entry to retrieve</param>
//     /// <returns>The EntryData for the entry</returns>
//     public EntryData GetEntry(string id)
//     {
//         return values[id];
//     }
// }

// internal static class GlobalBlackboard
// {
//     public static Dictionary<string, object> dict
//     {
//         get
//         {
//             if (!UnityEngine.Application.isPlaying)
//                 return null;

//             if (_dict == null)
//                 _dict = new Dictionary<string, object>();

//             return _dict;
//         }
//     }
//     private static Dictionary<string, object> _dict;
// }
