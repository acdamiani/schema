using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Schema.Internal;
using UnityEngine;

namespace Schema
{
    [Serializable]
    public class BlackboardEntrySelector<T> : BlackboardEntrySelector
    {
        [SerializeField] private T m_inspectorValue;

        /// <summary>
        ///     Create a blackboard entry selector
        /// </summary>
        public BlackboardEntrySelector() : base(typeof(T))
        {
        }

        /// <summary>
        ///     Create a blackboard entry selector with a given initial inspectorValue
        /// </summary>
        /// <param name="initialInspectorValue">Default inspector value of this selector</param>
        public BlackboardEntrySelector(T initialInspectorValue) : base(typeof(T))
        {
            m_inspectorValue = initialInspectorValue;
        }

        /// <summary>
        ///     Value of this selector (runtime only)
        /// </summary>
        public new T value
        {
            get
            {
                if (!Application.isPlaying)
                    return default;

                if (!isDynamic && entry == null)
                    return inspectorValue;

                object v = base.value;

                Type t = v?.GetType();

                if (t == null)
                    return default;

                if (typeof(T).IsAssignableFrom(t))
                    return (T)v;
                return (T)Convert.ChangeType(v, typeof(T));
            }
            set
            {
                if (!Application.isPlaying || (!isDynamic && entry == null))
                    return;

                base.value = value;
            }
        }

        /// <summary>
        ///     Value of the field in the inspector. Can be used to override the given value (e.g. to restrict an int to a certain
        ///     range)
        /// </summary>
        public T inspectorValue
        {
            get => m_inspectorValue;
            set => m_inspectorValue = value;
        }

        /// <summary>
        ///     Name of the entry referenced by this selector
        /// </summary>
        public new string name => GetName();

        private string GetName()
        {
            if (isDynamic)
                return $"${dynamicName}";
            if (entry != null)
                return $"${entry.name}";
            if (inspectorValue != null)
                return inspectorValue.ToString();

            return "$null";
        }
    }

    [Serializable]
    public class BlackboardEntrySelector
    {
        [SerializeField] private BlackboardEntry m_entry;
        [SerializeField] private string m_valuePath;
        [SerializeField] private string m_dynamicName;
        [SerializeField] private bool m_isDynamic;
        [SerializeField] private List<string> m_filters;
        private string lastEntryTypeString;

        /// <summary>
        ///     Whether the entry attached to this selector is null
        /// </summary>
        protected BlackboardEntrySelector(params Type[] filters)
        {
            m_filters = filters.Select(x => x.AssemblyQualifiedName).ToList();

#if UNITY_EDITOR
            Blackboard.entryListChanged += BlackboardChangedCallback;
            Blackboard.entryTypeChanged += VerifyType;
#endif
        }

        public BlackboardEntrySelector()
        {
            m_filters = new List<string>();

#if UNITY_EDITOR
            Blackboard.entryListChanged += BlackboardChangedCallback;
            Blackboard.entryTypeChanged += VerifyType;
#endif
        }

        /// <summary>
        ///     BlackboardEntry for this selector
        /// </summary>
        public BlackboardEntry entry => m_entry;

        /// <summary>
        ///     Name of the entry referenced by this selector
        /// </summary>
        public string name => GetName();

        /// <summary>
        ///     Whether this selector is dynamic
        /// </summary>
        public bool isDynamic => m_isDynamic;

        /// <summary>
        ///     The dynamic name for this selector
        /// </summary>
        public string dynamicName => m_dynamicName;

        public Type entryType => entry == null ? null : entry.type;

        /// <summary>
        ///     The value of this selector (runtime only)
        /// </summary>
        public object value
        {
            get
            {
                if (!Application.isPlaying || (!m_isDynamic && m_entry == null))
                    return null;

                if (m_isDynamic) return ExecutableTree.current.blackboard.GetDynamic(m_dynamicName);

                if (entry != null)
                {
                    object obj = ExecutableTree.current.blackboard.Get(m_entry);

                    if (obj == null)
                        return null;

                    string vPath = new Regex(@" \(.*\)$").Replace(m_valuePath, "");

                    if (!string.IsNullOrEmpty(m_valuePath))
                        return DynamicProperty.Get(obj, vPath);

                    return obj;
                }

                return null;
            }
            set
            {
                if (!Application.isPlaying || (!m_isDynamic && m_entry == null))
                    return;

                if (m_isDynamic)
                {
                    ExecutableTree.current.blackboard.SetDynamic(m_dynamicName, value);
                }
                else if (m_entry != null)
                {
                    if (!string.IsNullOrEmpty(m_valuePath.Trim('/')))
                    {
                        object valueObj = ExecutableTree.current.blackboard.Get(m_entry);

                        if (valueObj == null)
                            return;

                        DynamicProperty.Set(valueObj, m_valuePath, value);
                    }
                    else
                    {
                        ExecutableTree.current.blackboard.Set(m_entry, value);
                    }
                }
            }
        }

        private string GetName()
        {
            if (m_isDynamic)
                return $"${m_dynamicName}";
            if (m_entry != null)
                return $"${m_entry.name}";

            return "$null";
        }

        /// <summary>
        ///     Apply all possible filters for this selector
        /// </summary>
        public void ApplyAllFilters()
        {
            ApplyFilters(Blackboard.mappedBlackboardTypes);
        }

        /// <summary>
        ///     Overwrite the current list of filters with a new type filter
        /// </summary>
        /// <typeparam name="T">Type of the new filter</typeparam>
        public void ApplyFilter<T>()
        {
            ApplyFilter(typeof(T));
        }

        /// <summary>
        ///     Overwrite the current list of filters with two new type filters
        /// </summary>
        /// <typeparam name="T1">First type of the new filter</typeparam>
        /// <typeparam name="T2">Second type of the new filter</typeparam>
        public void ApplyFilters<T1, T2>()
        {
            ApplyFilters(typeof(T1), typeof(T2));
        }

        /// <summary>
        ///     Overwrite the current list of filters with a new type filter
        /// </summary>
        /// <param name="type">Type of the new filter</param>
        public void ApplyFilter(Type type)
        {
            ApplyFilters(type);
        }

        /// <summary>
        ///     Overwrite the current list of filters with a new list of filters
        /// </summary>
        /// <param name="filters">Array of filters to add</param>
        public void ApplyFilters(params Type[] filters)
        {
            ApplyFilters((IEnumerable<Type>)filters);
        }

        /// <summary>
        ///     Overwrite the current list of filters with a new list of filters
        /// </summary>
        /// <param name="filters">IEnumerable of filters to add</param>
        public void ApplyFilters(IEnumerable<Type> filters)
        {
            if (!Application.isEditor)
                return;

            Type t = GetType();

            if (t != typeof(BlackboardEntrySelector))
            {
                Debug.LogWarning(
                    "Applying filters to a generic selector or component selector is not allowed. To use custom filters, create a non-generic BlackboardEntrySelector instead."
                );
                return;
            }

            m_filters.Clear();

            if (entryType != null && !filters.Contains(EntryType.GetMappedType(entryType)))
                m_entry = null;

            m_filters = filters
                .Where(t =>
                {
                    bool b = Blackboard.mappedBlackboardTypes.Contains(t);

                    if (!b)
                        Debug.LogWarning($"Type {t.Name} is not a valid Blackboard type");

                    return b;
                })
                .Select(t => t.AssemblyQualifiedName)
                .ToList();
        }

        /// <summary>
        ///     Add a list of filters to the current list of filters
        /// </summary>
        /// <param name="filters">Array of filters to add</param>
        public void AddFilters(params Type[] filters)
        {
            AddFilters((IEnumerable<Type>)filters);
        }

        /// <summary>
        ///     Add a list of filters to the current list of filters
        /// </summary>
        /// <param name="filters">IEnumerable of filters to add</param>
        public void AddFilters(IEnumerable<Type> filters)
        {
            if (!Application.isEditor)
                return;

            Type t = GetType();

            if (t != typeof(BlackboardEntrySelector))
            {
                Debug.LogWarning(
                    "Adding filters to a generic selector or component selector is not allowed. To use custom filters, create a non-generic BlackboardEntrySelector instead."
                );
                return;
            }

            m_filters.AddRange(filters
                .Where(t =>
                {
                    bool b = Blackboard.mappedBlackboardTypes.Contains(t);

                    if (!b)
                        Debug.LogWarning($"Type {t.Name} is not a valid Blackboard type");

                    return b;
                })
                .Select(t => t.AssemblyQualifiedName)
            );
        }

        /// <summary>
        ///     Remove a list of filters from the current list of filters
        /// </summary>
        /// <param name="filters">Array of filters to remove</param>
        public void RemoveFilters(params Type[] filters)
        {
            RemoveFilters((IEnumerable<Type>)filters);
        }

        /// <summary>
        ///     Remove a list of filters from the current list of filters
        /// </summary>
        /// <param name="filters">Array of filters to remove</param>
        public void RemoveFilters(IEnumerable<Type> filters)
        {
            if (!Application.isEditor)
                return;

            Type t = GetType();

            if (t != typeof(BlackboardEntrySelector))
            {
                Debug.LogWarning(
                    "Removing filters from a generic selector or component selector is not allowed. To use custom filters, create a non-generic BlackboardEntrySelector instead."
                );
                return;
            }

            m_filters = m_filters.Except(filters
                .Where(t =>
                {
                    bool b = Blackboard.mappedBlackboardTypes.Contains(t);

                    if (!b)
                        Debug.LogWarning($"Type {t.Name} is not a valid Blackboard type");

                    return b;
                })
                .Select(t => t.AssemblyQualifiedName)
            ).ToList();

            if (entryType != null && filters.Contains(EntryType.GetMappedType(entryType)))
                m_entry = null;
        }
#if UNITY_EDITOR
        private void BlackboardChangedCallback(Blackboard changed)
        {
            VerifyResults(changed);
        }

        public void VerifyResults(Blackboard changed)
        {
            BlackboardEntry e = Array.Find(changed.entries, entry => this.entry == entry);

            if (e == null)
            {
                m_entry = null;
                m_valuePath = "";
            }
        }

        private void VerifyType(BlackboardEntry entry)
        {
            if (this.entry == entry && !m_filters.Contains(entry.typeString))
            {
                m_entry = null;
                m_valuePath = "";
            }
        }
#endif
    }
}