using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using Schema.Utilities;

[Serializable]
public class Blackboard : ScriptableObject
{
    public static Blackboard instance;
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
    public string[] entryByteStrings { get; private set; }
    private List<BlackboardEntrySelector> selectors = new List<BlackboardEntrySelector>();
    public Dictionary<Type, BlackboardEntry[]> typeArrays { get; private set; }
    void OnEnable()
    {
        Dictionary<Type, Color> copy = new Dictionary<Type, Color>(typeColors);
        foreach (Type key in copy.Keys)
        {
            typeColors[key] = key.Name.GetHashCode().ToString().ToColor();
        }

        foreach (BlackboardEntry entry in entries)
            entry.blackboard = this;

        entryByteStrings = GetEntryByteStrings();
    }
    public void ConnectSelector(BlackboardEntrySelector selector)
    {
        if (!selectors.Contains(selector))
        {
            selectors.Add(selector);

            selector.mask = GetMask(selector.filters);
            selector.UpdateEntry(this);
        }
    }
    public void UpdateSelectors()
    {
        //Remove any selectors that were removed since last update
        selectors.RemoveAll(selector => selector == null);

        foreach (BlackboardEntrySelector selector in selectors)
        {
            selector.UpdateEntry(this);
            selector.mask = GetMask(selector.filters);
        }

        if (typeArrays == null)
            typeArrays = new Dictionary<Type, BlackboardEntry[]>();

        foreach (Type t in typeColors.Keys)
        {
            IEnumerable<BlackboardEntry> entriesForType = entries.FindAll(entry => Type.GetType(entry.type) == t);
            typeArrays[t] = entriesForType.ToArray();
        }

        entryByteStrings = GetEntryByteStrings();
    }
    //Returns a bit mask given specific filters
    public int GetMask(List<string> filters)
    {
        List<Type> typeArray = filters.Select(s => Type.GetType(s)).ToList();

        int ret = 0;
        for (int i = entries.Count - 1; i >= 0; i--)
        {
            bool entryIncluded = typeArray.Contains(Type.GetType(entries[i].type));

            if (entryIncluded)
                ret |= 1 << i;
        }

        return ret;
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
    private string[] GetEntryByteStrings()
    {
        string[] ret = new string[entries.Count];

        for (int i = ret.Length - 1; i >= 0; i--)
        {
            BlackboardEntry entry = entries[i];

            byte[] guidBytes = System.Text.Encoding.ASCII.GetBytes(entry.uID);
            byte[] nameBytes = System.Text.Encoding.ASCII.GetBytes(entry.Name);

            string s = Convert.ToBase64String(guidBytes.Concat(nameBytes).ToArray());

            ret[ret.Length - i - 1] = s;
        }

        return ret;
    }
}