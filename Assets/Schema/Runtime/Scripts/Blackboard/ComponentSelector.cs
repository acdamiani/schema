using System;
using UnityEngine;

[Serializable]
public class ComponentSelector<T> : Schema.Internal.ComponentSelector where T : Component
{
    [SerializeField] public bool useSelf;
    [SerializeField] private T fieldValue;
    [SerializeField] private string fieldValueType = typeof(T).AssemblyQualifiedName;
    public T GetValue(SchemaAgent agent)
    {
        T component;
        if (useSelf)
            component = agent.GetComponent<T>();
        else
            component = value.GetComponent<T>();

        return component;
    }
}

namespace Schema.Internal
{
    [Serializable]
    public class ComponentSelector : BlackboardEntrySelector<GameObject> { }
}