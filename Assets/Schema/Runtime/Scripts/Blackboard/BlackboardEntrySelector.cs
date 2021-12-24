using System;
using System.Collections.Generic;
using UnityEngine;
using Schema.Runtime;
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
    public List<string> filters = new List<string>();
    public BlackboardEntrySelector()
    {
        if (Blackboard.instance != null)
            Blackboard.instance.ConnectSelector(this);
    }
    public void UpdateEntry(Blackboard blackboard)
    {
        List<BlackboardEntry> validEntries = new List<BlackboardEntry>();

        validEntries = blackboard.entries
            .FindAll(entry => filters.Select(item => Type.GetType(item)).Contains(Type.GetType(entry.type)));

        int index = validEntries
            .FindIndex(x => x.uID == entryID);

        entryID = (index == -1 ? GetDefaultEntry(blackboard) : validEntries[index])?.uID;

        this.blackboard = blackboard;
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