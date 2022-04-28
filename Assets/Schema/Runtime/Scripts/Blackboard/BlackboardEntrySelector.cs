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
    /// Value of this selector
    /// </summary>
    public new T value
    {
        get
        {
            if (String.IsNullOrEmpty(entryID))
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
    private Blackboard blackboard;
    [SerializeField] private int mask;
    [SerializeField] private int blackboardTypesMask = -1;
    [SerializeField] private string entryName;
    public string entryID
    {
        get { return m_entryID; }
    }
    [SerializeField] protected string m_entryID;
    [SerializeField] private string valuePath;
    public Type entryType
    {
        get
        {
            if (String.IsNullOrEmpty(entryID))
                return null;

            if (_entryType == null || lastEntryTypeString != entryTypeString)
            {
                _entryType = Type.GetType(entryTypeString);
                lastEntryTypeString = entryTypeString;
            }

            return _entryType;
        }
    }
    private Type _entryType;
    private string lastEntryTypeString;
    [SerializeField] private string entryTypeString;
    public object value
    {
        get
        {
            if (String.IsNullOrEmpty(entryID))
                return null;

            object obj = BlackboardDataContainer.Get(entryID, SchemaManager.pid);

            if (obj == null)
                return null;

            if (!String.IsNullOrEmpty(valuePath))
                return DynamicProperty.Get(obj, valuePath);
            else
                return obj;
        }
        set
        {
            if (String.IsNullOrEmpty(entryID))
                return;

            if (!String.IsNullOrEmpty(valuePath))
                DynamicProperty.Set(this.value, valuePath, value);
            else
                BlackboardDataContainer.Set(entryID, SchemaManager.pid, value);
        }
    }
    public List<string> filters;
    public bool empty => String.IsNullOrEmpty(entryID);
    protected BlackboardEntrySelector(params Type[] filters)
    {
        this.filters = filters.Select(x => x.AssemblyQualifiedName).ToList();

        Blackboard.entryListChanged += BlackboardChangedCallback;
        Blackboard.entryTypeChanged += VerifyType;
    }
    public BlackboardEntrySelector()
    {
        this.filters = new List<string>();

        Blackboard.entryListChanged += BlackboardChangedCallback;
        Blackboard.entryTypeChanged += VerifyType;
    }
    private void BlackboardChangedCallback(Blackboard changed)
    {
        VerifyResults(changed);

        System.Tuple<int, int> masks = changed.GetMask(filters);
        mask = masks.Item1;
        blackboardTypesMask = masks.Item2;
    }
    public void VerifyResults(Blackboard changed)
    {
        BlackboardEntry e = changed.entries.Find(entry => entry.uID == entryID);

        if (e == null)
        {
            m_entryID = "";
            valuePath = "";
        }
    }
    private void VerifyType(BlackboardEntry entry)
    {
        entryTypeString = entry.typeString;
        _entryType = null;

        if (entryID == entry.uID && !filters.Contains(entry.typeString))
        {
            m_entryID = "";
            valuePath = "";
        }
    }
    public void ClearFilters()
    {
        filters.Clear();
    }
    public void AddNumericFilter()
    {
        AddFilter<int>();
        AddFilter<float>();
    }
    public void AddGameObjectFilter()
    {
        AddFilter<GameObject>();
    }
    public void AddFilter<T>()
    {
        if (Blackboard.typeColors.ContainsKey(typeof(T)) && !filters.Contains(typeof(T).AssemblyQualifiedName))
        {
            filters.Add(typeof(T).AssemblyQualifiedName);
            System.Tuple<int, int> masks = Blackboard.instance.GetMask(filters);
            mask = masks.Item1;
            blackboardTypesMask = masks.Item2;
        }
    }
    public void AddFilter(Type type)
    {
        if (Blackboard.typeColors.ContainsKey(type) && !filters.Contains(type.AssemblyQualifiedName))
        {
            filters.Add(type.AssemblyQualifiedName);
            System.Tuple<int, int> masks = Blackboard.instance.GetMask(filters);
            mask = masks.Item1;
            blackboardTypesMask = masks.Item2;
        }
    }
    public void AddAllFilters()
    {
        foreach (Type t in Blackboard.typeColors.Keys)
        {
            AddFilter(t);
        }
    }
    public void AddIntFilter()
    {
        AddFilter<int>();
    }
    public void AddFloatFilter()
    {
        AddFilter<float>();
    }
    public void AddStringFilter()
    {
        AddFilter<string>();
    }
    public void AddBoolFilter()
    {
        AddFilter<bool>();
    }
    public void AddVector2Filter()
    {
        AddFilter<Vector2>();
    }
    public void AddVector3Filter()
    {
        AddFilter<Vector3>();
    }
    public void AddVector4Filter()
    {
        AddFilter<Vector4>();
    }
    public void AddQuaternionFilter()
    {
        AddFilter<Quaternion>();
    }
}
[System.AttributeUsage(AttributeTargets.Field)]
public class WriteOnlyAttribute : System.Attribute { }
[System.AttributeUsage(AttributeTargets.Field)]
public class DisableDynamicBindingAttribute : System.Attribute { }