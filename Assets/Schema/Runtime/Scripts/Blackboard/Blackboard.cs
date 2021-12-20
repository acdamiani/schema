using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using Schema;

[Serializable]
public class Blackboard : ScriptableObject
{
    public static readonly Dictionary<Type, Color> typeColors = new Dictionary<Type, Color>() {
        { typeof(int), Color.black },
        { typeof(string), Color.black },
        { typeof(float), Color.black },
        { typeof(bool), Color.black },
        { typeof(Enum), Color.black },
        { typeof(Quaternion), Color.black },
        { typeof(Vector2), Color.black },
        { typeof(Vector3), Color.black },
        { typeof(Matrix4x4), Color.black },
        { typeof(Type), Color.black },
        { typeof(GameObject), Color.black }
    };
    public List<BlackboardEntry> entries = new List<BlackboardEntry>();
    private List<BlackboardEntrySelector> selectors = new List<BlackboardEntrySelector>();

    void OnEnable()
    {
        Dictionary<Type, Color> copy = new Dictionary<Type, Color>(typeColors);
        foreach (Type key in copy.Keys)
        {
            typeColors[key] = key.Name.GetHashCode().ToString().ToColor();
        }

        foreach (BlackboardEntry entry in entries)
            entry.blackboard = this;
    }
    public void ConnectSelector(BlackboardEntrySelector selector)
    {
        if (!selectors.Contains(selector))
        {
            selectors.Add(selector);
            selector.UpdateEntry(this);
        }
    }
    public void UpdateSelectors()
    {
        //Remove any selectors that were removed since last update
        selectors.RemoveAll(selector => selector == null);

        Debug.Log("Updating selectors...");

        foreach (BlackboardEntrySelector selector in selectors)
            selector.UpdateEntry(this);
    }
    public BlackboardEntry GetEntry(string uID)
    {
        if (entries != null)
            return entries.Find(x => x != null && x.uID.Equals(uID));
        else
            return null;
    }
    public void AddEntry(Type type)
    {
        BlackboardEntry entry = ScriptableObject.CreateInstance<BlackboardEntry>();
        entry.blackboard = this;

        entry.Name = UniqueName(type.Name + "Key", entries.Select(e => e.Name).ToList());
        entry.type = type.AssemblyQualifiedName;

        entry.hideFlags = HideFlags.HideAndDontSave;

        //Adding entry

        entries.Add(entry);

        UpdateSelectors();
    }
    private string UniqueName(string desiredName, List<string> names)
    {
        int i = 0;

        while (names.Contains(desiredName + (i == 0 ? "" : i.ToString())))
        {
            i++;
        }

        return desiredName + (i == 0 ? "" : i.ToString());
    }
    public void RemoveEntry(int index)
    {
        if (index > entries.Count - 1) return;

        BlackboardEntry obj = entries[index];
        entries.Remove(obj);
        DestroyImmediate(obj);

        UpdateSelectors();
    }
}