using System;
using System.Collections.Generic;
using System.Reflection;

namespace Schema.Internal
{
    internal static class BlackboardDataContainer
    {
        public static void Initialize(Blackboard blackboard)
        {
            foreach (BlackboardEntry entry in blackboard.entries)
            {
                if (!values.ContainsKey(entry))
                    values.Add(entry, new EntryData(entry));
            }
        }
        private static Dictionary<BlackboardEntry, EntryData> values = new Dictionary<BlackboardEntry, EntryData>();
        private static Dictionary<string, EntryData> dynamicValues = new Dictionary<string, EntryData>();
        public static object GetDynamic(string name)
        {
            dynamicValues.TryGetValue(name, out EntryData data);

            if (data != null && (SchemaManager.currentNode.index < data.position.Item1 || SchemaManager.currentNode.index > data.position.Item1 + data.position.Item2))
            {
                dynamicValues.Remove(name);

                return null;
            }

            return data?.GetValue(SchemaManager.pid);
        }
        public static void SetDynamic(string name, object value)
        {
            UnityEngine.Debug.Log("setting");

            if (!dynamicValues.ContainsKey(name))
                dynamicValues[name] = new EntryData(name, SchemaManager.currentParentNode.index, SchemaManager.currentParentNode.breadth);

            dynamicValues[name].SetValue(SchemaManager.pid, value);
        }
        public static object Get(BlackboardEntry entry, int pid)
        {
            values.TryGetValue(entry, out EntryData data);

            return data?.GetValue(pid);
        }
        public static void Set(BlackboardEntry entry, int pid, object value)
        {
            values.TryGetValue(entry, out EntryData data);

            data?.SetValue(pid, value);
        }
        internal class EntryData
        {
            public List<object> value = new List<object>();
            private object defaultValue;
            public BlackboardEntry.EntryType type;
            public Tuple<int, int> position = new Tuple<int, int>(-1, -1);
            public EntryData(BlackboardEntry entry)
            {
                type = entry.entryType;
                defaultValue = entry.type.IsValueType ? Activator.CreateInstance(entry.type) : null;
                value.Add(defaultValue);
            }
            public EntryData(object defaultValue, int index, int breadth)
            {
                Type t = defaultValue.GetType();

                type = BlackboardEntry.EntryType.Local;
                this.defaultValue = t.IsValueType ? Activator.CreateInstance(t) : null;
                value.Add(this.defaultValue);

                position = new Tuple<int, int>(index, breadth);
            }
            public object GetValue(int pid)
            {
                if (type == BlackboardEntry.EntryType.Local)
                {
                    if (pid > value.Count - 1)
                    {
                        while (pid > value.Count - 1)
                            value.Add(defaultValue);
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
                            value.Add(null);

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
}