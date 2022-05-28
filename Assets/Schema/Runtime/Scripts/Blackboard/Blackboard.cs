using UnityEngine;
using System;
using System.Reflection;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Schema.Utilities;
using UnityEditor;

namespace Schema.Internal
{
    [Serializable]
    public class Blackboard : ScriptableObject
    {
        public static Blackboard instance;
        public delegate void EntryListChangedCallback(Blackboard changed);
        public delegate void EntryTypeChangedCallback(BlackboardEntry changed);
        public static event EntryListChangedCallback entryListChanged;
        public static event EntryTypeChangedCallback entryTypeChanged;
        public static Type[] blackboardTypes { get { return _blackboardTypes == null ? _blackboardTypes = GetBlackboardTypes() : _blackboardTypes; } }
        private static Type[] _blackboardTypes;
        /// <summary>
        /// Array of entries for the Blackboard
        /// </summary>
        public BlackboardEntry[] entries { get { return m_entries; } }
        [SerializeField] private BlackboardEntry[] m_entries = Array.Empty<BlackboardEntry>();
        void OnEnable()
        {
            foreach (BlackboardEntry entry in entries)
                entry.blackboard = this;
        }
        private static Type[] GetBlackboardTypes()
        {
            return HelperMethods.GetEnumerableOfType(typeof(Schema.EntryType)).ToArray();
        }
        public System.Tuple<int, int> GetMask(List<string> filters)
        {
            List<Type> typeArray = filters.Select(s => Type.GetType(s)).ToList();

            int ret1 = 0;
            for (int i = entries.Length - 1; i >= 0; i--)
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
            entry.name = UniqueName(HelperMethods.GetFriendlyTypeName(type) + "Key", entries.Select(e => e.name).ToList());
            entry.type = type;
            entry.hideFlags = HideFlags.HideInHierarchy;

            if (!String.IsNullOrEmpty(AssetDatabase.GetAssetPath(this)))
                AssetDatabase.AddObjectToAsset(entry, this);

            if (undo)
            {
                Undo.RegisterCreatedObjectUndo(entry, actionName);
                Undo.RegisterCompleteObjectUndo(this, actionName);
            }

            ArrayUtility.Add(ref m_entries, entry);

            entryListChanged?.Invoke(this);
        }
        private string UniqueName(string desiredName, List<string> names)
        {
            int i = 0;

            while (names.Contains(desiredName + (i == 0 ? "" : i.ToString())))
                i++;

            return desiredName + (i == 0 ? "" : i.ToString());
        }
        public void RemoveEntry(BlackboardEntry entry, string actionName = "Remove Entry", bool undo = true)
        {
            if (undo)
            {
                Undo.RegisterCompleteObjectUndo(this, actionName);
                ArrayUtility.Remove(ref m_entries, entry);
                Undo.DestroyObjectImmediate(entry);
            }
            else
            {
                ArrayUtility.Remove(ref m_entries, entry);
                ScriptableObject.DestroyImmediate(entry, true);
            }

            entryListChanged?.Invoke(this);
        }
        public void RemoveEntry(int index, string actionName = "Remove Entry", bool undo = true)
        {
            if (index > entries.Length - 1) return;

            BlackboardEntry entry = entries[index];

            if (undo)
            {
                Undo.RegisterCompleteObjectUndo(this, actionName);
                ArrayUtility.Remove(ref m_entries, entry);
                Undo.DestroyObjectImmediate(entry);
            }
            else
            {
                ArrayUtility.Remove(ref m_entries, entry);
                ScriptableObject.DestroyImmediate(entry, true);
            }

            entryListChanged?.Invoke(this);
        }
        public static void InvokeEntryTypeChanged(BlackboardEntry entry)
        {
            entryTypeChanged?.Invoke(entry);
        }
    }
}