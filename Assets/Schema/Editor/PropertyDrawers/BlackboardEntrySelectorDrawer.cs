using UnityEngine;
using UnityEditor;
using Schema.Runtime;
using System;
using System.Linq;
using System.Collections.Generic;

[CustomPropertyDrawer(typeof(BlackboardEntrySelector), true)]
public class BlackboardEntrySelectorDrawer : PropertyDrawer
{
    [Serializable]
    struct EntryData
    {
        public string name;
        public string id;
        public Type type;
        public EntryData(string name, string id, Type type)
        {
            this.name = name;
            this.id = id;
            this.type = type;
        }
    }
    [SerializeField] private SerializableDictionary<string, int> indices = new SerializableDictionary<string, int>();
    [SerializeField] private EntryData[] entryData;
    private string[] entryByteStrings;
    private Blackboard blackboard;
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty mask = property.FindPropertyRelative("mask");
        SerializedProperty entryID = property.FindPropertyRelative("entryID");

        //If the entryNames changes, recalculate the mask, and the cached array as well
        if (entryData == null || entryByteStrings == null || !entryByteStrings.SequenceEqual(Blackboard.instance.entryByteStrings))
        {
            Debug.Log("Recalculating");

            mask.intValue = Blackboard.instance.GetMask(GetFilters(property.FindPropertyRelative("filters")));

            entryData = new EntryData[Blackboard.instance.entryByteStrings.Length];

            for (int i = 0; i < entryData.Length; i++)
            {
                string name = GetName(Blackboard.instance.entryByteStrings[i]);
                string id = GetID(Blackboard.instance.entryByteStrings[i]);
                entryData[i] = new EntryData(name, id, null);
            }

            if (entryByteStrings == null || entryByteStrings.Length != Blackboard.instance.entryByteStrings.Length)
                entryByteStrings = new string[Blackboard.instance.entryByteStrings.Length];

            Array.Copy(Blackboard.instance.entryByteStrings, entryByteStrings, Blackboard.instance.entryByteStrings.Length);
        }

        EntryData[] filteredOptions = FilterArrayByMask(entryData, mask.intValue);
        GUIContent[] contentOptions = new GUIContent[filteredOptions.Length];

        int curIndex = -1;

        for (int j = 0; j < filteredOptions.Length; j++)
        {
            if (filteredOptions[j].id == entryID.stringValue)
                curIndex = j;
        }

        curIndex = curIndex == -1 ? filteredOptions.Length - 1 : curIndex;

        for (int i = 0; i < filteredOptions.Length; i++)
            contentOptions[i] = new GUIContent(filteredOptions[i].name);

        EditorGUI.BeginProperty(position, label, property);

        int newIndex = EditorGUI.Popup(position, label, curIndex, contentOptions);

        entryID.stringValue = filteredOptions[newIndex].id;

        EditorGUI.EndProperty();
    }
    private List<string> GetFilters(SerializedProperty property)
    {
        List<string> filters = new List<string>();

        for (int i = 0; i < property.arraySize; i++)
        {
            SerializedProperty propAtIndex = property.GetArrayElementAtIndex(i);
            filters.Add(propAtIndex.stringValue);
        }

        return filters;
    }
    private string GetName(string s)
    {
        byte[] bytes = Convert.FromBase64String(s);
        byte[] nameBytes = new byte[bytes.Length - 32];

        for (int i = 32; i < bytes.Length; i++)
            nameBytes[i - 32] = bytes[i];

        return System.Text.Encoding.ASCII.GetString(nameBytes);
    }
    private string GetID(string s)
    {
        byte[] bytes = Convert.FromBase64String(s);
        byte[] idBytes = new byte[32];

        for (int i = 0; i < 32; i++)
            idBytes[i] = bytes[i];

        return System.Text.Encoding.ASCII.GetString(idBytes);
    }
    private T[] FilterArrayByMask<T>(T[] array, int mask)
    {
        T[] ret = new T[Mathf.Clamp(BitCount(mask), 0, array.Length)];

        if (ret.Length == 0)
            return ret;

        int j = ret.Length - 1;
        for (int i = array.Length - 1; i >= 0; i--)
        {
            bool isIncluded = (mask & (1 << (array.Length - i - 1))) != 0;

            if (isIncluded)
            {
                ret[j] = array[i];
                j--;
            }
        }

        return ret;
    }
    int BitCount(int u)
    {
        int count = 0;
        while (u != 0)
        {
            u = u & (u - 1);
            count++;
        }
        return count;
    }
}