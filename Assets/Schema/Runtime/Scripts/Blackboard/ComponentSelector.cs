using System;
using UnityEngine;

[Serializable]
public class ComponentSelector<T> : Schema.Internal.ComponentSelectorBase where T : Component
{
    [SerializeField] private bool useSelf = true;
    [SerializeField] private T fieldValue;
    [SerializeField] private string fieldValueType = typeof(T).AssemblyQualifiedName;
    private T cache;
    public T GetValue(SchemaAgent agent)
    {
        if (cache == null)
        {
            if (useSelf)
                cache = agent.GetComponent<T>();
            else if (fieldValue != null)
                cache = fieldValue;
            else
                return value?.GetComponent<T>();
        }
        else
        {
            return cache;
        }

        return cache;
    }
}

namespace Schema.Internal
{
    [Serializable]
    public class ComponentSelectorBase : BlackboardEntrySelector<GameObject> { }
}