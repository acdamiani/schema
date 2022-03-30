using System;
using System.Collections.Generic;
using UnityEngine;
using Schema.Runtime;
using Schema.Utilities;
using System.IO;
using System.Linq;

[Serializable]
public class BlackboardEntrySelector<T> : BlackboardEntrySelector
{
    public new T value
    {
        get
        {
            if (_value != null && String.IsNullOrEmpty(entryID))
                return (T)_value;

            return (T)base.value;
        }
        set
        {
            base.value = value;
        }
    }
    [SerializeField] private T _value;
    public BlackboardEntrySelector() : base(typeof(T)) { }
}
[Serializable]
public class BlackboardEntrySelector
{
    private Blackboard blackboard;
    public int mask;
    public int blackboardTypesMask = -1;
    public string entryName;
    public string entryID;
    public string valuePath;
    public Type entryType
    {
        get
        {
            return Type.GetType(entryTypeString);
        }
        set
        {
            entryTypeString = value.AssemblyQualifiedName;
        }
    }
    [SerializeField] private string entryTypeString;
    public object value
    {
        get
        {
            object obj = BlackboardDataContainer.Get(entryID, SchemaManager.pid);

            if (!String.IsNullOrEmpty(valuePath))
                return DynamicProperty.Get(obj, valuePath);
            else
                return obj;
        }
        set
        {
            BlackboardDataContainer.Set(entryID, SchemaManager.pid, value);
        }
    }
    public List<string> filters;
    public bool empty => String.IsNullOrEmpty(entryID);
    public BlackboardEntrySelector(params Type[] filters)
    {
        this.filters = filters.Select(x => x.AssemblyQualifiedName).ToList();

        Blackboard.entryListChanged += BlackboardChangedCallback;
    }
    public BlackboardEntrySelector()
    {
        this.filters = new List<string>();

        Blackboard.entryListChanged += BlackboardChangedCallback;
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
        if (!changed.entries.Find(entry => entry.uID == entryID))
        {
            entryID = "";
            valuePath = "";
        }
    }
    /// <summary>
    /// Gets the referenced Blackboard Entry by this selector. This is only available in the Editor. 
    /// To get values and other information during runtime, use the BlackboardData class.
    /// </summary>
    /// <returns>The Editor-only Blackboard Entry referenced by this selector</returns>
    public BlackboardEntry GetEditorEntry()
    {
        return String.IsNullOrEmpty(entryID) ? null : blackboard?.GetEntry(entryID);
    }
    private string GetName(string s)
    {
        byte[] bytes = Convert.FromBase64String(s);
        byte[] nameBytes = new byte[bytes.Length - 32];

        for (int i = 32; i < bytes.Length; i++)
            nameBytes[i - 32] = bytes[i];

        return System.Text.Encoding.ASCII.GetString(nameBytes);
    }
    private string GetID(string s)
    {
        byte[] bytes = Convert.FromBase64String(s);
        byte[] idBytes = new byte[32];

        for (int i = 0; i < 32; i++)
            idBytes[i] = bytes[i];

        return System.Text.Encoding.ASCII.GetString(idBytes);
    }
    BlackboardEntry GetDefaultEntry(Blackboard blackboard)
    {
        List<BlackboardEntry> entries = blackboard.entries.FindAll(entry => filters.Select(item => Type.GetType(item)).Contains(Type.GetType(entry.typeString)));

        if (entries.Count == 0) return null;
        else return entries[0];
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
    public void AddQuaternionFilter()
    {
        AddFilter<Quaternion>();
    }
}
[System.AttributeUsage(AttributeTargets.Field)]
public class WriteOnlyAttribute : System.Attribute { }
[System.AttributeUsage(AttributeTargets.Field)]
public class DisableDynamicBindingAttribute : System.Attribute { }