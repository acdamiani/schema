using System;
using UnityEngine;

namespace Schema.Internal
{
    [Serializable]
    public abstract class ComponentSelectorBase : BlackboardEntrySelector<GameObject>
    {
        /// <summary>
        ///     The gameObject entry to get the component from, null if useSelf is true (read only)
        /// </summary>
        public new GameObject value => base.value;
    }
}