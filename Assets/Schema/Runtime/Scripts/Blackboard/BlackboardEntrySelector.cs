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

            if (entry == null)
                return (T)inspectorValue;

            T v = (T)base.value;

            return v;
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
    public BlackboardEntrySelector() : base(typeof(T)) { }
}
[Serializable]
public class BlackboardEntrySelector
{
    [SerializeField] private BlackboardEntry m_entry;
    /// <summary>
    /// BlackboardEntry for this selector
    /// </summary>
    public BlackboardEntry entry { get { return m_entry; } }
    [SerializeField] private int m_mask = -1;
    [SerializeField] private string m_valuePath;
    public Type entryType
    {
        get
        {
            return entry?.type;
        }
    }
    private string lastEntryTypeString;
    /// <summary>
    /// The value of this selector (runtime only)
    /// </summary>
    public object value
    {
        get
        {
            if (entry == null || !Application.isPlaying)
                return null;

            object obj = BlackboardDataContainer.Get(entry, SchemaManager.pid);

            if (obj == null)
                return null;

            if (!String.IsNullOrEmpty(m_valuePath))
                return DynamicProperty.Get(obj, m_valuePath);
            else
                return obj;
        }
        set
        {
            if (entry == null || !Application.isPlaying)
                return;

            if (!String.IsNullOrEmpty(m_valuePath))
                DynamicProperty.Set(this.value, m_valuePath, value);
            else
                BlackboardDataContainer.Set(entry, SchemaManager.pid, value);
        }
    }
    [SerializeField] private List<string> m_filters;
    /// <summary>
    /// Whether the entry attached to this selector is null
    /// </summary>
    public bool empty => entry == null;
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
    public void ApplyAllFilters()
    {
        ApplyFilters(Blackboard.typeColors.Keys);
    }
    /// <summary>
    /// Overwrite the current list of filters with a new type filter
    /// </summary>
    /// <typeparam name="T">Type of the new filter</typeparam>
    public void ApplyFilter<T>()
    {
        ApplyFilter(typeof(T));
    }
    /// <summary>
    /// Overwrite the current list of filters with a new type filter
    /// </summary>
    /// <param name="type">Type of the new filter</param>
    public void ApplyFilter(Type type)
    {
        ApplyFilters(type);
    }
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
        m_filters.Clear();
        if (!filters.Contains(entryType))
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
    { }
    /// <summary>
    /// Remove a list of filters from the current list of filters
    /// </summary>
    /// <param name="filters">Array of filters to remove</param>
    public void RemoveFilters(params Type[] filters) { RemoveFilters((IEnumerable<Type>)filters); }
    /// <summary>
    /// Remove a list of filters from the current list of filters
    /// </summary>
    /// <param name="filters">Array of filters to remove</param>
    public void RemoveFilters(IEnumerable<Type> filters) { }
}
[System.AttributeUsage(AttributeTargets.Field)]
public class WriteOnlyAttribute : System.Attribute { }
[System.AttributeUsage(AttributeTargets.Field)]
public class DisableDynamicBindingAttribute : System.Attribute { }