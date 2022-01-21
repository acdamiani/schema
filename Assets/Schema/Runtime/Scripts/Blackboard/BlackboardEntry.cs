using System;
using UnityEngine;

[Serializable]
public class BlackboardEntry : ScriptableObject
{
    public string Name
    {
        get
        {
            return _name;
        }
        set
        {
            _name = value;
            name = value;
            blackboard.UpdateSelectors();
        }
    }
    [SerializeField] private string _name;
    public string description;
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
            blackboard.UpdateSelectors();
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
    public BlackboardEntry()
    {
        if (string.IsNullOrEmpty(uID)) uID = Guid.NewGuid().ToString("N");
    }

}