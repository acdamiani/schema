using UnityEngine;

namespace Schema
{
    public static class ComponentSelectorExtensions
    {
        /// <summary>
        ///     Get component of type using a ComponentSelector
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
        ///     Get component of type using a ComponentSelector
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