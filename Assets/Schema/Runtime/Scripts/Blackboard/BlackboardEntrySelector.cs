using System;
using System.Collections.Generic;
using UnityEngine;
using Schema.Runtime;
using Schema.Utilities;
using System.Linq;

/// <summary>
///	Field to accept Blackboard Keys as inputs
/// </summary>
[Serializable]
public class BlackboardEntrySelector
{
    private Blackboard blackboard;
    public int mask;
    public string entryName;
    public string entryID;
    public List<string> filters;
    public BlackboardEntrySelector(params Type[] filters)
    {
        this.filters = filters.Select(x => x.AssemblyQualifiedName).ToList();

        if (Blackboard.instance != null)
            Blackboard.instance.ConnectSelector(this);
    }
    public BlackboardEntrySelector()
    {
        this.filters = new List<string>();

        if (Blackboard.instance != null)
            Blackboard.instance.ConnectSelector(this);
    }
    public void UpdateEntry(Blackboard blackboard)
    {
        if (blackboard.entries.FindIndex(entry => entry.uID == entryID) == -1)
        {
            entryID = "";
            entryName = "";
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
        List<BlackboardEntry> entries = blackboard.entries.FindAll(entry => filters.Select(item => Type.GetType(item)).Contains(Type.GetType(entry.type)));

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
        Debug.Log("Adding filter");
        AddFilter<GameObject>();
    }
    public void AddFilter<T>()
    {
        if (Blackboard.typeColors.ContainsKey(typeof(T)) && !filters.Contains(typeof(T).AssemblyQualifiedName))
        {
            filters.Add(typeof(T).AssemblyQualifiedName);
        }
    }
    public void AddFilter(Type type)
    {
        if (Blackboard.typeColors.ContainsKey(type) && !filters.Contains(type.AssemblyQualifiedName))
        {
            filters.Add(type.AssemblyQualifiedName);
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
public class EntryFiltersAttribute : System.Attribute
{
    public Type[] validTypes;
    public EntryFiltersAttribute(params Type[] types)
    {
        List<Type> vT = new List<Type>();

        for (int i = 0; i < types.Length; i++)
        {
            if (Blackboard.typeColors.ContainsKey(types[i]) && !vT.Contains(types[i]))
            {
                vT.Add(types[i]);
            }
        }

        validTypes = vT.ToArray();
    }
}