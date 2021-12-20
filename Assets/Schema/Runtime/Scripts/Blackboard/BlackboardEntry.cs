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
            blackboard.UpdateSelectors();
        }
    }
    [SerializeField] private string _name;
    public string description;
    public string type
    {
        get
        {
            return _type;
        }
        set
        {
            _type = value;
            blackboard.UpdateSelectors();
        }
    }
    [SerializeField] private string _type;
    [SerializeField] public string uID;
    public Blackboard blackboard;
    public BlackboardEntry()
    {
        if (string.IsNullOrEmpty(uID)) uID = Guid.NewGuid().ToString("N");
    }

}