using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Schema.Utilities;
using UnityEditor;
using UnityEngine;

namespace Schema.Internal
{
    [Serializable]
    public class Blackboard : ScriptableObject
    {
        private static Type[] _blackboardTypes;
        private static Type[] _mappedBlackboardTypes;
        [SerializeField] private BlackboardEntry[] m_entries = Array.Empty<BlackboardEntry>();

        public static Type[] blackboardTypes =>
            _blackboardTypes == null ? _blackboardTypes = GetBlackboardTypes() : _blackboardTypes;

        public static Type[] mappedBlackboardTypes => _mappedBlackboardTypes == null
            ? _mappedBlackboardTypes = GetMappedBlackboardTypes()
            : _mappedBlackboardTypes;

        /// <summary>
        ///     Array of entries for the Blackboard
        /// </summary>
        public BlackboardEntry[] entries => m_entries;

        private void OnEnable()
        {
            foreach (BlackboardEntry entry in entries)
                entry.blackboard = this;
        }

        private static Type[] GetBlackboardTypes()
        {
            return HelperMethods.GetEnumerableOfType(typeof(EntryType)).ToArray();
        }

        private static Type[] GetMappedBlackboardTypes()
        {
            return blackboardTypes.Select(x => EntryType.GetMappedType(x)).ToArray();
        }
        public static Blackboard global => _global == null ? _global = LoadGlobal() : _global;
        private static Blackboard _global;

        private static Blackboard LoadGlobal()
        {
            Blackboard loaded = Resources.Load<Blackboard>("GlobalBlackboard");

            #if UNITY_EDITOR
            if (loaded == null)
            {
                loaded = CreateInstance<Blackboard>();

                DirectoryInfo res = new DirectoryInfo(Path.Combine(Application.dataPath, "Resources"));
                res.Create();

                AssetDatabase.CreateAsset(loaded, "Assets/Resources/GlobalBlackboard.asset");
            }
            #endif

            return loaded;
        }


        #if UNITY_EDITOR
        public static Blackboard instance;

        public delegate void EntryListChangedCallback(Blackboard changed);

        public delegate void EntryTypeChangedCallback(BlackboardEntry changed);

        public static event EntryListChangedCallback entryListChanged;
        public static event EntryTypeChangedCallback entryTypeChanged;
        #endif
        #if UNITY_EDITOR
        public int GetTypeMask(IEnumerable<string> filters)
        {
            List<Type> typeArray = filters.Select(s => Type.GetType(s)).ToList();

            int mask = 0;
            for (int i = mappedBlackboardTypes.Length - 1; i >= 0; i--)
            {
                bool entryIncluded = typeArray.Contains(mappedBlackboardTypes[i]);

                if (entryIncluded)
                    mask |= 1 << i;
            }

            return mask;
        }

        /// <summary>
        ///     Add an entry to the Blackboard
        /// </summary>
        /// <param name="type">Type of entry to add</param>
        /// <param name="actionName">Name of the undo action</param>
        /// <param name="undo">Whether to register this operation in the undo stack</param>
        public void AddEntry(Type type, string actionName = "Add Entry", bool undo = true)
        {
            BlackboardEntry entry = CreateInstance<BlackboardEntry>();
            entry.blackboard = this;
            entry.name = UniqueName(type.GetFriendlyTypeName() + "Key", entries.Select(e => e.name).ToList());
            entry.type = type;
            entry.hideFlags = HideFlags.HideInHierarchy;

            if (!string.IsNullOrEmpty(AssetDatabase.GetAssetPath(this)))
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
                DestroyImmediate(entry, true);
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
                DestroyImmediate(entry, true);
            }

            entryListChanged?.Invoke(this);
        }

        public static void InvokeEntryTypeChanged(BlackboardEntry entry)
        {
            entryTypeChanged?.Invoke(entry);
        }
        #endif
    }
}