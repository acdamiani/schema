using System;
using System.Collections.Generic;
using System.Reflection;

namespace Schema.Internal
{
    public class ExecutableBlackboard
    {
        private Dictionary<BlackboardEntry, EntryData> values = new Dictionary<BlackboardEntry, EntryData>();
        private Dictionary<string, EntryData> dynamicValues = new Dictionary<string, EntryData>();
        public ExecutableBlackboard(Blackboard blackboard)
        {
            for (int i = 0; i < blackboard.entries.Length; i++)
            {
                BlackboardEntry entry = blackboard.entries[i];

                if (!values.ContainsKey(entry))
                    values.Add(entry, new EntryData(entry));
            }
        }
        public object GetDynamic(string name)
        {
            ExecutionContext current = ExecutionContext.current;

            dynamicValues.TryGetValue(name, out EntryData data);

            if (data != null && (current.node.index < data.position.Item1 || current.node.index >= data.position.Item1 + data.position.Item2))
            {
                dynamicValues.Remove(name);

                return null;
            }

            return data?.GetValue(current.agent.GetInstanceID());
        }
        public void SetDynamic(string name, object value)
        {
            ExecutionContext current = ExecutionContext.current;

            if (!dynamicValues.ContainsKey(name))
                dynamicValues[name] = new EntryData(value.GetType(), current.node.index, current.node.breadth);

            dynamicValues[name].SetValue(current.agent.GetInstanceID(), value);
        }
        public object Get(BlackboardEntry entry)
        {
            values.TryGetValue(entry, out EntryData data);

            return data?.GetValue(ExecutionContext.current.agent.GetInstanceID());
        }
        public void Set(BlackboardEntry entry, object value)
        {
            values.TryGetValue(entry, out EntryData data);

            data?.SetValue(ExecutionContext.current.agent.GetInstanceID(), value);
        }
        internal class EntryData
        {
            private Dictionary<int, object> values;
            private object defaultValue;
            public Tuple<int, int> position = new Tuple<int, int>(-1, -1);
            public EntryData(BlackboardEntry entry)
            {
                Type mapped = EntryType.GetMappedType(entry.type);

                defaultValue = mapped.IsValueType ? Activator.CreateInstance(mapped) : null;
                values = new Dictionary<int, object>();
            }
            public EntryData(Type defaultValueType, int index, int breadth)
            {
                this.defaultValue = defaultValueType.IsValueType ? Activator.CreateInstance(defaultValueType) : null;

                position = new Tuple<int, int>(index, breadth);
            }
            public object GetValue(int pid)
            {
                values.TryGetValue(pid, out object value);

                return value ?? defaultValue;
            }
            public void SetValue(int pid, object v)
            {
                values[pid] = v;
            }
        }
    }
}