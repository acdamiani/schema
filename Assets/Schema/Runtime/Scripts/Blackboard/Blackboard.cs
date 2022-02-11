using UnityEngine;
using System;
using System.Reflection;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Schema.Utilities;
using UnityEditor;

[Serializable]
public class Blackboard : ScriptableObject
{
    public static Blackboard instance;
    public delegate void EntryListChangedCallback(Blackboard changed);
    public static event EntryListChangedCallback entryListChanged;
    public static readonly Type[] blackboardTypes = {
        typeof(int),
        typeof(string),
        typeof(float),
        typeof(bool),
        typeof(Enum),
        typeof(Quaternion),
        typeof(Vector2),
        typeof(Vector3),
        typeof(Matrix4x4),
        typeof(Type),
        typeof(GameObject),
        typeof(AnimationCurve)
    };
    public static readonly Dictionary<Type, Color> typeColors = new Dictionary<Type, Color>() {
        { typeof(int),            Color.black },
        { typeof(string),         Color.black },
        { typeof(float),          Color.black },
        { typeof(bool),           Color.black },
        { typeof(Enum),           Color.black },
        { typeof(Quaternion),     Color.black },
        { typeof(Vector2),        Color.black },
        { typeof(Vector3),        Color.black },
        { typeof(Matrix4x4),      Color.black },
        { typeof(Type),           Color.black },
        { typeof(GameObject),     Color.black },
        { typeof(AnimationCurve), Color.black }
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

        entryListChanged?.Invoke(this);
    }
    //Returns a bit mask given specific filters
    public System.Tuple<int, int> GetMask(List<string> filters)
    {
        List<Type> typeArray = filters.Select(s => Type.GetType(s)).ToList();

        int ret1 = 0;
        for (int i = entries.Count - 1; i >= 0; i--)
        {
            bool entryIncluded = typeArray.Contains(entries[i].type);

            if (entryIncluded)
                ret1 |= 1 << i;
        }

        int ret2 = 0;
        for (int i = blackboardTypes.Length - 1; i >= 0; i--)
        {
            bool entryIncluded = typeArray.Contains(blackboardTypes[i]);

            if (entryIncluded)
                ret2 |= 1 << i;
        }

        return new System.Tuple<int, int>(ret1, ret2);
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
        entry.typeString = type.AssemblyQualifiedName;

        entry.hideFlags = HideFlags.HideAndDontSave;

        entries.Add(entry);

        entryByteStrings = GetEntryByteStrings();
        entryListChanged?.Invoke(this);
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
    public void RemoveEntry(BlackboardEntry entry, bool preserve)
    {
        entries.Remove(entry);

        if (!preserve)
            DestroyImmediate(entry);

        entryByteStrings = GetEntryByteStrings();
        entryListChanged?.Invoke(this);
    }
    public void RemoveEntry(int index)
    {
        if (index > entries.Count - 1) return;

        BlackboardEntry obj = entries[index];
        entries.Remove(obj);
        DestroyImmediate(obj);

        entryByteStrings = GetEntryByteStrings();
        entryListChanged?.Invoke(this);
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