using System;
using UnityEngine;

namespace Schema
{
    [Serializable]
    public sealed class ComponentSelector<T> : Schema.Internal.ComponentSelectorBase where T : Component
    {
        public bool useSelf { get { return m_useSelf; } }
        [SerializeField] private bool m_useSelf = true;
        [SerializeField] private string m_fieldValueType = typeof(T).AssemblyQualifiedName;
        private T cache;
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
    public static class ComponentSelectorExtensions
    {
        /// <summary>
        /// Get component of type using a ComponentSelector
        /// </summary>
        /// <typeparam name="T">Type of component to retrieve. Must match type of selector</typeparam>
        /// <param name="component">Component to retreive the other component from</param>
        /// <param name="selector">Selector to use to get component</param>
        /// <returns>Component retrived by the method</returns>
        public static T GetComponent<T>(this Component component, ComponentSelector<T> selector) where T : Component
        {
            return selector.GetValue(component);
        }
        /// <summary>
        /// Get component of type using a ComponentSelector
        /// </summary>
        /// <typeparam name="T">Type of component to retrieve. Must match type of selector</typeparam>
        /// <param name="gameObject">GameObject to retreive the other component from</param>
        /// <param name="selector">Selector to use to get component</param>
        /// <returns>Component retrived by the method</returns>
        public static T GetComponent<T>(this GameObject gameObject, ComponentSelector<T> selector) where T : Component
        {
            return selector.GetValue(gameObject);
        }
    }
}

namespace Schema.Internal
{
    [Serializable]
    public abstract class ComponentSelectorBase : BlackboardEntrySelector<GameObject>
    {
        /// <summary>
        /// The gameObject entry to get the component from, null if useSelf is true (read only)
        /// </summary>
        public new GameObject value { get { return base.value; } }
    }
}
