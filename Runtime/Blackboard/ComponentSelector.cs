using System;
using Schema.Internal;
using Schema.Utilities;
using UnityEngine;

namespace Schema
{
    [Serializable]
    public sealed class ComponentSelector<T> : ComponentSelectorBase where T : Component
    {
        [SerializeField] private bool m_useSelf = true;
        [SerializeField] private string m_fieldValueType = typeof(T).AssemblyQualifiedName;
        private CacheDictionary<int, T> cache = new CacheDictionary<int, T>();
        public bool useSelf => m_useSelf;

        public T GetValue(GameObject gameObject)
        {
            int id;
            // Get component from gameObject, not underlying value
            if (useSelf)
            {
                id = gameObject.GetInstanceID();
                return cache.GetOrCreate(id, gameObject.GetComponent<T>);
            }

            if (!value) return null;

            // Get component from underlying value
            id = value.GetInstanceID();
            return cache.GetOrCreate(id, value.GetComponent<T>);
        }

        public T GetValue(Component component)
        {
            return GetValue(component.gameObject);
        }
    }
}