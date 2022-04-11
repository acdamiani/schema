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
    public delegate void EntryTypeChangedCallback(BlackboardEntry changed);
    public static event EntryListChangedCallback entryListChanged;
    public static event EntryTypeChangedCallback entryTypeChanged;
    public static readonly Type[] blackboardTypes = {
        typeof(int),
        typeof(string),
        typeof(float),
        typeof(bool),
        typeof(Quaternion),
        typeof(Vector2),
        typeof(Vector3),
        typeof(Vector4),
        typeof(Matrix4x4),
        typeof(GameObject),
        typeof(AnimationCurve)
    };
    public static readonly Dictionary<Type, Color> typeColors = new Dictionary<Type, Color>() {
        { typeof(int),            Color.black },
        { typeof(string),         Color.black },
        { typeof(float),          Color.black },
        { typeof(bool),           Color.black },
        { typeof(Quaternion),     Color.black },
        { typeof(Vector2),        Color.black },
        { typeof(Vector3),        Color.black },
        { typeof(Vector4),        Color.black },
        { typeof(Matrix4x4),      Color.black },
        { typeof(GameObject),     Color.black },
        { typeof(AnimationCurve), Color.black }
    };
    public List<BlackboardEntry> entries = new List<BlackboardEntry>();
    public string[] entryByteStrings { get; private set; }
    void OnEnable()
    {
        Dictionary<Type, Color> copy = new Dictionary<Type, Color>(typeColors);
        foreach (Type key in copy.Keys)
            typeColors[key] = key.Name.GetHashCode().ToString().ToColor();

        foreach (BlackboardEntry entry in entries)
            entry.blackboard = this;

        entryByteStrings = GetEntryByteStrings();
        //
    }
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
    /// <summary>
    /// Add an entry to the Blackboard
    /// </summary>
    /// <param name="type">Type of entry to add</param>
    /// <param name="actionName">Name of the undo action</param>
    /// <param name="undo">Whether to register this operation in the undo stack</param>
    public void AddEntry(Type type, string actionName = "Add Entry", bool undo = true)
    {
        BlackboardEntry entry = ScriptableObject.CreateInstance<BlackboardEntry>();
        entry.blackboard = this;
        entry.Name = UniqueName(type.Name + "Key", entries.Select(e => e.Name).ToList());
        entry.typeString = type.AssemblyQualifiedName;

        if (undo)
        {
            Undo.RegisterCreatedObjectUndo(entry, actionName);
            Undo.RegisterCompleteObjectUndo(this, actionName);
        }

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
    public void RemoveEntry(BlackboardEntry entry, string actionName = "Remove Entry", bool undo = true)
    {
        if (undo)
        {
            Undo.RegisterCompleteObjectUndo(this, actionName);
            entries.Remove(entry);
            Undo.DestroyObjectImmediate(entry);
        }
        else
        {
            entries.Remove(entry);
            ScriptableObject.DestroyImmediate(entry, true);
        }

        entryByteStrings = GetEntryByteStrings();
        entryListChanged?.Invoke(this);
    }
    public void RemoveEntry(int index, string actionName = "Remove Entry", bool undo = true)
    {
        if (index > entries.Count - 1) return;

        BlackboardEntry entry = entries[index];

        if (undo)
        {
            Undo.RegisterCompleteObjectUndo(this, actionName);
            entries.Remove(entry);
            Undo.DestroyObjectImmediate(entry);
        }
        else
        {
            entries.Remove(entry);
            ScriptableObject.DestroyImmediate(entry, true);
        }

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
    public static void InvokeEntryTypeChanged(BlackboardEntry entry)
    {
        entryTypeChanged?.Invoke(entry);
    }
}