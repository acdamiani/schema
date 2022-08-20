using System;
using System.Collections.Generic;

namespace Schema.Utilities
{
    public class CacheDictionary<T1, T2>
    {
        private readonly Dictionary<T1, T2> dict = new Dictionary<T1, T2>();

        public T2 GetOrCreate(T1 key, Func<T2> @default)
        {
            if (!dict.TryGetValue(key, out T2 val))
                return dict[key] = @default();
            return val;
        }

        public void Set(T1 key, T2 value)
        {
            dict[key] = value;
        }
    }
}