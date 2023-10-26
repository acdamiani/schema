using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     A Unity SeralizableDictionary:
///     see https://answers.unity.com/questions/460727/how-to-serialize-dictionary-with-unity-serializati.html
/// </summary>
[Serializable]
public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
{
    [SerializeField] private List<TKey> keys = new List<TKey>();

    [SerializeField] private List<TValue> values = new List<TValue>();

    // save the dictionary to lists
    public void OnBeforeSerialize()
    {
        keys.Clear();
        values.Clear();
        foreach (KeyValuePair<TKey, TValue> pair in this)
        {
            keys.Add(pair.Key);
            values.Add(pair.Value);
        }
    }

    // load dictionary from lists
    public void OnAfterDeserialize()
    {
        Clear();

        if (keys.Count != values.Count)
            throw new Exception(string.Format(
                "there are {0} keys and {1} values after deserialization. Make sure that both key and value types are serializable."));

        for (int i = keys.Count - 1; i >= 0; i--)
            try
            {
                Add(keys[i], values[i]);
            }
            catch
            {
            }
    }
}