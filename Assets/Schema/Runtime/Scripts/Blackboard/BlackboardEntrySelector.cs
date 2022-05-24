using System;
using System.Collections.Generic;
using UnityEngine;
using Schema;
using Schema.Utilities;
using System.IO;
using System.Linq;

[Serializable]
public class BlackboardEntrySelector<T> : BlackboardEntrySelector
{
    /// <summary>
    /// Value of this selector (runtime only)
    /// </summary>
    public new T value
    {
        get
        {
            if (!Application.isPlaying)
                return default(T);

            if (entry == null && !isDynamic)
                return (T)inspectorValue;

            try { return (T)base.value; }
            catch { return default(T); }
        }
        set
        {
            base.value = value;
        }
    }
    /// <summary>
    /// Value of the field in the inspector. Can be used to override the given value (e.g. to restrict an int to a certain range)
    /// </summary>
    public T inspectorValue { get { return m_inspectorValue; } set { m_inspectorValue = value; } }
    [SerializeField] private T m_inspectorValue;
    /// <summary>
    /// Create a blackboard entry selector
    /// </summary>
    public BlackboardEntrySelector() : base(typeof(T)) { }
    /// <summary>
    /// Create a blackboard entry selector with a given initial inspectorValue
    /// </summary>
    /// <param name="initialInspectorValue">Default inspector value of this selector</param>
    public BlackboardEntrySelector(T initialInspectorValue) : base(typeof(T)) { m_inspectorValue = initialInspectorValue; }
}
[Serializable]
public class BlackboardEntrySelector
{
    [SerializeField] private BlackboardEntry m_entry;
    /// <summary>
    /// BlackboardEntry for this selector
    /// </summary>
    public BlackboardEntry entry { get { return m_entry; } }
    /// <summary>
    /// Name of the entry referenced by this selector
    /// </summary>
    public string entryName { get { return m_isDynamic ? m_dynamicName : m_entry?.name; } }
    [SerializeField] private string m_valuePath;
    [SerializeField] private string m_dynamicName;
    /// <summary>
    /// Whether this selector is dynamic
    /// </summary>
    public bool isDynamic { get { return m_isDynamic; } }
    /// <summary>
    /// The dynamic name for this selector
    /// </summary>
    public string dynamicName { get { return m_dynamicName; } }
    [SerializeField] private bool m_isDynamic;
    public Type entryType { get { return entry?.type; } }
    private string lastEntryTypeString;
    /// <summary>
    /// The value of this selector (runtime only)
    /// </summary>
    public object value
    {
        get
        {
            if (!Application.isPlaying)
                return null;

            if (m_isDynamic)
            {
                object obj = BlackboardDataContainer.GetDynamic(m_dynamicName);

                return obj;
            }
            else if (entry != null)
            {
                object obj = BlackboardDataContainer.Get(entry, SchemaManager.pid);

                if (obj == null)
                    return null;

                if (!String.IsNullOrEmpty(m_valuePath))
                    return DynamicProperty.Get(obj, m_valuePath);
                else
                    return obj;
            }

            return null;
        }
        set
        {
            if (!Application.isPlaying)
                return;

            if (m_isDynamic)
            {
                BlackboardDataContainer.SetDynamic(m_dynamicName, value);
            }
            else if (entry != null)
            {
                if (!String.IsNullOrEmpty(m_valuePath.Trim('/')))
                {
                    object valueObj = BlackboardDataContainer.Get(entry, SchemaManager.pid);

                    if (valueObj == null)
                        return;

                    DynamicProperty.Set(valueObj, m_valuePath, value);
                }
                else
                {
                    BlackboardDataContainer.Set(entry, SchemaManager.pid, value);
                }
            }
        }
    }
    [SerializeField] private List<string> m_filters;
    /// <summary>
    /// Whether the entry attached to this selector is null
    /// </summary>
    protected BlackboardEntrySelector(params Type[] filters)
    {
        this.m_filters = filters.Select(x => x.AssemblyQualifiedName).ToList();

        Blackboard.entryListChanged += BlackboardChangedCallback;
        Blackboard.entryTypeChanged += VerifyType;
    }
    public BlackboardEntrySelector()
    {
        this.m_filters = new List<string>();

        Blackboard.entryListChanged += BlackboardChangedCallback;
        Blackboard.entryTypeChanged += VerifyType;
    }
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
    /// <summary>
    /// Apply all possible filters for this selector
    /// </summary>
    public void ApplyAllFilters() { ApplyFilters(Blackboard.typeColors.Keys); }
    /// <summary>
    /// Overwrite the current list of filters with a new type filter
    /// </summary>
    /// <typeparam name="T">Type of the new filter</typeparam>
    public void ApplyFilter<T>() { ApplyFilter(typeof(T)); }
    /// <summary>
    /// Overwrite the current list of filters with two new type filters
    /// </summary>
    /// <typeparam name="T1">First type of the new filter</typeparam>
    /// <typeparam name="T2">Second type of the new filter</typeparam>
    public void ApplyFilters<T1, T2>() { ApplyFilters(typeof(T1), typeof(T2)); }
    /// <summary>
    /// Overwrite the current list of filters with a new type filter
    /// </summary>
    /// <param name="type">Type of the new filter</param>
    public void ApplyFilter(Type type) { ApplyFilters(type); }
    /// <summary>
    /// Overwrite the current list of filters with a new list of filters
    /// </summary>
    /// <param name="filters">Array of filters to add</param>
    public void ApplyFilters(params Type[] filters) { ApplyFilters((IEnumerable<Type>)filters); }
    /// <summary>
    /// Overwrite the current list of filters with a new list of filters
    /// </summary>
    /// <param name="filters">IEnumerable of filters to add</param>
    public void ApplyFilters(IEnumerable<Type> filters)
    {
        Type t = this.GetType();

        if (t != typeof(BlackboardEntrySelector))
        {
            Debug.LogWarning(
                "Applying filters to a generic selector or component selector is not allowed. To use custom filters, create a non-generic BlackboardEntrySelector instead."
            );
            return;
        }

        m_filters.Clear();

        if (entryType != null && !filters.Contains(entryType))
            m_entry = null;

        this.m_filters = filters
            .Where(t =>
            {
                bool b = Blackboard.typeColors.ContainsKey(t);

                if (!b)
                    Debug.LogWarning($"Type {t.Name} is not a valid Blackboard type");

                return b;

            })
            .Select(t => t.AssemblyQualifiedName)
            .ToList();
    }
    /// <summary>
    /// Add a list of filters to the current list of filters
    /// </summary>
    /// <param name="filters">Array of filters to add</param>
    public void AddFilters(params Type[] filters) { AddFilters((IEnumerable<Type>)filters); }
    /// <summary>
    /// Add a list of filters to the current list of filters
    /// </summary>
    /// <param name="filters">IEnumerable of filters to add</param>
    public void AddFilters(IEnumerable<Type> filters)
    {
        Type t = this.GetType();

        if (t != typeof(BlackboardEntrySelector))
        {
            Debug.LogWarning(
                "Adding filters to a generic selector or component selector is not allowed. To use custom filters, create a non-generic BlackboardEntrySelector instead."
            );
            return;
        }

        this.m_filters.AddRange(filters
            .Where(t =>
            {
                bool b = Blackboard.typeColors.ContainsKey(t);

                if (!b)
                    Debug.LogWarning($"Type {t.Name} is not a valid Blackboard type");

                return b;
            })
            .Select(t => t.AssemblyQualifiedName)
            );
    }
    /// <summary>
    /// Remove a list of filters from the current list of filters
    /// </summary>
    /// <param name="filters">Array of filters to remove</param>
    public void RemoveFilters(params Type[] filters) { RemoveFilters((IEnumerable<Type>)filters); }
    /// <summary>
    /// Remove a list of filters from the current list of filters
    /// </summary>
    /// <param name="filters">Array of filters to remove</param>
    public void RemoveFilters(IEnumerable<Type> filters)
    {
        Type t = this.GetType();

        if (t != typeof(BlackboardEntrySelector))
        {
            Debug.LogWarning(
                "Removing filters from a generic selector or component selector is not allowed. To use custom filters, create a non-generic BlackboardEntrySelector instead."
            );
            return;
        }

        this.m_filters = this.m_filters.Except(filters
            .Where(t =>
            {
                bool b = Blackboard.typeColors.ContainsKey(t);

                if (!b)
                    Debug.LogWarning($"Type {t.Name} is not a valid Blackboard type");

                return b;
            })
            .Select(t => t.AssemblyQualifiedName)
            ).ToList();

        if (entryType != null && filters.Contains(entryType))
            m_entry = null;
    }
}
[System.AttributeUsage(AttributeTargets.Field)]
public class WriteOnlyAttribute : System.Attribute { }
[System.AttributeUsage(AttributeTargets.Field)]
public class DisableDynamicBindingAttribute : System.Attribute { }