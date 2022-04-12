using System;
using UnityEngine;

/// <summary>
/// ScriptableObject representation for BlackboardEntry
/// </summary>
[Serializable]
public class BlackboardEntry : ScriptableObject
{
    [UnityEngine.TextArea] public string description;
    public string typeString
    {
        get
        {
            return _typeString;
        }
        set
        {
            _typeString = value;
            _type = Type.GetType(_typeString);
        }
    }
    [SerializeField] private string _typeString;
    public Type type
    {
        get
        {
            if (_type == null)
                _type = Type.GetType(_typeString);

            return _type;
        }
        set
        {
            _typeString = value.AssemblyQualifiedName;
            _type = value;
        }
    }
    private Type _type;
    public string uID;
    public Blackboard blackboard;
    public EntryType entryType = EntryType.Local;
    public BlackboardEntry()
    {
        if (string.IsNullOrEmpty(uID)) uID = Guid.NewGuid().ToString("N");
    }
    public enum EntryType
    {
        Local,
        Shared,
        Global
    }
}