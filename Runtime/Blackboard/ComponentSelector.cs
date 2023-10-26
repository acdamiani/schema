using System;
using Schema.Internal;
using UnityEngine;

namespace Schema
{
    [Serializable]
    public sealed class ComponentSelector<T> : ComponentSelectorBase where T : Component
    {
        [SerializeField] private bool m_useSelf = true;
        [SerializeField] private string m_fieldValueType = typeof(T).AssemblyQualifiedName;
        private T cache;
        public bool useSelf => m_useSelf;

        public T GetValue(GameObject gameObject)
        {
            if (cache == null)
            {
                if (useSelf)
                    cache = gameObject.GetComponent<T>();
                else
                    return value?.GetComponent<T>();
            }
            else
            {
                return cache;
            }

            return cache;
        }

        public T GetValue(Component component)
        {
            return GetValue(component.gameObject);
        }
    }
}