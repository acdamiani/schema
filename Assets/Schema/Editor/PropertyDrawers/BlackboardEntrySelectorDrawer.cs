using UnityEngine;
using UnityEditor;
using Schema.Utilities;
using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using Schema;
using Schema.Internal;

namespace SchemaEditor
{
    [CustomPropertyDrawer(typeof(BlackboardEntrySelector), true)]
    public class BlackboardEntrySelectorDrawer : PropertyDrawer
    {
        private static Dictionary<string, SelectorPropertyInfo> info = new Dictionary<string, SelectorPropertyInfo>();
        private delegate void GUIDelayCall(SerializedProperty property);
        private static event GUIDelayCall guiDelayCall;
        private static readonly Type[] valid = new Type[] { typeof(Schema.Node), typeof(Schema.Decorator) };
        private static Dictionary<Type, Type> typeMappings = new Dictionary<Type, Type>();
        private static Dictionary<Type, Tuple<string[], Type[]>> excluded = new Dictionary<Type, Tuple<string[], Type[]>>();
        private class SelectorPropertyInfo
        {
            public bool writeOnly;
            public float scroll;
        }
        public static void DoSelectorMenu(Rect position, SerializedProperty property, FieldInfo fieldInfo)
        {
            if (GUI.Button(position, Styles.menu, EditorStyles.miniButtonRight))
            {
                GenericMenu menu = GenerateMenu(property, fieldInfo);

                menu.DropDown(position);
            }
        }
        public static string DoSelectorDrawer(Rect position, SerializedProperty property, GUIContent label, Type fieldType, FieldInfo fieldInfo)
        {
            Type parentType = property.serializedObject.targetObject.GetType();

            if (!valid.Any(t => t.IsAssignableFrom(parentType)))
            {
                GUI.Label(position, new GUIContent("Cannot use a BlackboardEntrySelector in a non tree type", Styles.warnIcon), EditorStyles.miniLabel);
                return "";
            }
            else if (Blackboard.instance == null)
            {
                GUI.Label(position, new GUIContent("Cannot use a BlackboardEntrySelector outside a tree", Styles.warnIcon), EditorStyles.miniLabel);
                return "";
            }

            SerializedProperty entry = property.FindPropertyRelative("m_entry");
            SerializedProperty valuePathProp = property.FindPropertyRelative("m_valuePath");
            SerializedProperty value = property.FindPropertyRelative("m_inspectorValue");
            SerializedProperty isDynamicProperty = property.FindPropertyRelative("m_isDynamic");
            SerializedProperty dynamicPropertyName = property.FindPropertyRelative("m_dynamicName");

            if (!info.ContainsKey(property.propertyPath))
            {
                SelectorPropertyInfo pi = new SelectorPropertyInfo();
                pi.writeOnly = fieldInfo.GetCustomAttribute<WriteOnlyAttribute>() != null;
                info[property.propertyPath] = pi;
            }

            bool lastWideMode = EditorGUIUtility.wideMode;
            EditorGUIUtility.wideMode = true;

            Vector2 size = EditorStyles.miniButtonRight.CalcSize(new GUIContent(Styles.menu));

            Rect enumRect = new Rect(position.x, position.y, position.width - size.x, Mathf.Min(position.height, EditorGUIUtility.singleLineHeight));
            Rect textRect = new Rect(position.x, position.y + enumRect.height, position.width, enumRect.height);
            Rect buttonRect = new Rect(position.xMax - size.x, position.y, size.x, Mathf.Min(position.height, EditorGUIUtility.singleLineHeight));

            Vector2 oldIconSize = EditorGUIUtility.GetIconSize();
            EditorGUIUtility.SetIconSize(new Vector2(12, 12));

            EditorGUI.BeginProperty(position, label, property);

            bool doesHavePath = entry.objectReferenceValue != null;

            BlackboardEntry entryValue = null;

            if (doesHavePath)
                entryValue = (BlackboardEntry)entry.objectReferenceValue;

            if (isDynamicProperty.boolValue)
            {
                EditorGUI.PropertyField(enumRect, dynamicPropertyName, label, true);

                if (GUI.Button(buttonRect, Styles.menu, EditorStyles.miniButtonRight))
                {
                    GenericMenu menu = GenerateMenu(property, fieldInfo);

                    menu.DropDown(buttonRect);
                }
            }
            else if (value != null && !info[property.propertyPath].writeOnly)
            {
                EditorGUI.BeginDisabledGroup(doesHavePath);

                EditorGUI.PropertyField(enumRect, value, label, true);

                EditorGUI.EndDisabledGroup();

                if (doesHavePath)
                {
                    Rect p = EditorGUI.PrefixLabel(textRect, new GUIContent("\0"));
                    p.y += 3f;

                    GUIContent content = new GUIContent($"Using {entryValue.name}{valuePathProp.stringValue.Replace('/', '.')}");
                    size = EditorStyles.miniLabel.CalcSize(content);

                    GUI.BeginClip(p, new Vector2(info[property.propertyPath].scroll, 0f), Vector2.zero, false);

                    EditorGUI.LabelField(new Rect(0f, 3f, size.x, size.y), content, EditorStyles.miniLabel);

                    GUI.EndClip();

                    GUI.Box(p, GUIContent.none, EditorStyles.helpBox);

                    if (size.x > p.width && p.Contains(Event.current.mousePosition) && Event.current.type == EventType.ScrollWheel)
                    {
                        info[property.propertyPath].scroll = Mathf.Clamp(info[property.propertyPath].scroll - Event.current.delta.y * 10, -(size.x - p.width), 0f);
                        // Prevent scroll
                        Event.current.delta = Vector2.zero;
                    }
                }

                if (GUI.Button(buttonRect, Styles.menu, EditorStyles.miniButtonRight))
                {
                    GenericMenu menu = GenerateMenu(property, fieldInfo);

                    menu.DropDown(buttonRect);
                }
            }
            else
            {
                Rect controlRect = EditorGUI.PrefixLabel(position, label);

                string path = valuePathProp.stringValue;

                GUIContent buttonValue = new GUIContent(entryValue == null ? "None" : entryValue.name + valuePathProp.stringValue.Replace('/', '.').TrimEnd('.'));

                if (EditorGUI.DropdownButton(controlRect, buttonValue, FocusType.Passive))
                {
                    GenericMenu menu = GenerateMenu(property, fieldInfo);

                    menu.DropDown(controlRect);
                }
            }

            guiDelayCall?.Invoke(property);

            EditorGUI.EndProperty();

            EditorGUIUtility.SetIconSize(oldIconSize);

            EditorGUIUtility.wideMode = lastWideMode;

            string entryName = entry.objectReferenceValue?.name + valuePathProp.stringValue.TrimEnd('/').Replace('/', '.');

            if (!String.IsNullOrEmpty(entryName))
                return entryName;

            if (isDynamicProperty.boolValue)
                return "dynamic";
            else if (value != null)
                return "asset";

            return "";
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            DoSelectorDrawer(position, property, label, null, fieldInfo);
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty valueProp = property.FindPropertyRelative("m_inspectorValue");
            SerializedProperty entry = property.FindPropertyRelative("m_entry");

            bool lastWideMode = EditorGUIUtility.wideMode;
            EditorGUIUtility.wideMode = true;

            float height;

            if (!info.ContainsKey(property.propertyPath))
            {
                SelectorPropertyInfo pi = new SelectorPropertyInfo();
                pi.writeOnly = fieldInfo.GetCustomAttribute<WriteOnlyAttribute>() != null;
                info[property.propertyPath] = pi;
            }

            if (valueProp != null && !info[property.propertyPath].writeOnly)
                height = EditorGUI.GetPropertyHeight(valueProp, label, true) + (entry.objectReferenceValue != null ? EditorGUIUtility.singleLineHeight + 4 : 0);
            else
                height = base.GetPropertyHeight(property, label);

            EditorGUIUtility.wideMode = lastWideMode;

            return height;
        }
        private static string GetID(string s)
        {
            byte[] bytes = Convert.FromBase64String(s);
            byte[] idBytes = new byte[32];

            for (int i = 0; i < 32; i++)
                idBytes[i] = bytes[i];

            return System.Text.Encoding.ASCII.GetString(idBytes);
        }

        private static GenericMenu GenerateMenu(SerializedProperty property, FieldInfo fieldInfo)
        {
            SerializedProperty entry = property.FindPropertyRelative("m_entry");
            SerializedProperty valuePathProp = property.FindPropertyRelative("m_valuePath");
            SerializedProperty isDynamicProperty = property.FindPropertyRelative("m_isDynamic");

            bool isDynamicPropertyValue = isDynamicProperty.boolValue;

            GenericMenu menu = new GenericMenu();

            List<string> filtersList = new List<string>();

            SerializedProperty filters = property.FindPropertyRelative("m_filters");

            for (int i = 0; i < filters.arraySize; i++)
                filtersList.Add(filters.GetArrayElementAtIndex(i).stringValue);

            int mask = Blackboard.instance.GetTypeMask(filtersList);

            List<Type> filtered = HelperMethods.FilterArrayByMask(Blackboard.mappedBlackboardTypes.Reverse().ToArray(), mask)
                .ToList();

            filtered.ForEach(Debug.Log);

            menu.AddItem("None", entry.objectReferenceValue == null && !isDynamicPropertyValue, () => GenericMenuSelectOption(property, null), false);
            menu.AddSeparator("");
            menu.AddItem("Dynamic", isDynamicPropertyValue, () => ToggleDynamic(property), false);

            bool disableDynamicBinding = fieldInfo.GetCustomAttribute<DisableDynamicBindingAttribute>() != null;

            Dictionary<Type, IEnumerable<string>> tmp = new Dictionary<Type, IEnumerable<string>>();

            foreach (BlackboardEntry bEntry in Blackboard.instance.entries)
            {
                if (disableDynamicBinding)
                {
                    typeMappings.TryGetValue(bEntry.type, out Type value);

                    if (value == null)
                        value = EntryType.GetMappedType(bEntry.type);

                    if (!filtered.Any(t => value.IsAssignableFrom(t)))
                        continue;

                    menu.AddItem(
                        bEntry.name + (filtered.Count > 1 ? " (" + bEntry.type.Name + ")" : ""),
                        entry.objectReferenceValue == bEntry,
                        () => GenericMenuSelectOption(property, bEntry, "/"),
                        false
                    );
                }
                else
                {
                    typeMappings.TryGetValue(bEntry.type, out Type value);

                    if (value == null)
                        value = typeMappings[bEntry.type] = EntryType.GetMappedType(bEntry.type);

                    excluded.TryGetValue(bEntry.type, out Tuple<string[], Type[]> e);

                    if (e == null)
                        e = excluded[bEntry.type] = new Tuple<string[], Type[]>(EntryType.GetExcludedPaths(bEntry.type), EntryType.GetExcludedTypes(bEntry.type));

                    tmp.TryGetValue(bEntry.type, out IEnumerable<string> enumerated);

                    if (enumerated == null)
                    {
                        enumerated = tmp[bEntry.type] = EnumerateProperties(
                            value,
                            filtered,
                            e.Item1,
                            e.Item2,
                            needsSetter: info.ContainsKey(property.propertyPath) && info[property.propertyPath].writeOnly,
                            showType: filtered.Count > 1
                        );
                    }

                    foreach (string ss in enumerated)
                    {
                        menu.AddItem(
                            bEntry.name + ss,
                            valuePathProp.stringValue.Equals(ss) && entry.objectReferenceValue == bEntry,
                            () => GenericMenuSelectOption(property, bEntry, ss),
                            false
                        );
                    }
                }
            }

            return menu;
        }
        private static void GenericMenuSelectOption(SerializedProperty property, BlackboardEntry entry, string path = "")
        {
            SerializedProperty entryProp = property.FindPropertyRelative("m_entry");
            SerializedProperty valuePathProperty = property.FindPropertyRelative("m_valuePath");
            SerializedProperty isDynamicProperty = property.FindPropertyRelative("m_isDynamic");

            entryProp.objectReferenceValue = entry;
            valuePathProperty.stringValue = path;
            isDynamicProperty.boolValue = false;

            guiDelayCall += UpdateChanged;

            property.serializedObject.ApplyModifiedProperties();
        }
        private static void ToggleDynamic(SerializedProperty property)
        {
            SerializedProperty entryProp = property.FindPropertyRelative("m_entry");
            SerializedProperty valuePathProperty = property.FindPropertyRelative("m_valuePath");
            SerializedProperty isDynamicProperty = property.FindPropertyRelative("m_isDynamic");

            entryProp.objectReferenceValue = null;
            valuePathProperty.stringValue = "";
            isDynamicProperty.boolValue = !isDynamicProperty.boolValue;

            guiDelayCall += UpdateChanged;

            property.serializedObject.ApplyModifiedProperties();
        }
        // Reorderable list resize fix
        private static void UpdateChanged(SerializedProperty property)
        {
            GUI.changed = true;

            guiDelayCall -= UpdateChanged;
        }
        static int i;
        private static IEnumerable<string> EnumerateProperties(
            Type type,
            IEnumerable<Type> targets,
            IEnumerable<string> excludePaths,
            IEnumerable<Type> excludeTypes,
            string path = "",
            bool needsSetter = false,
            bool showType = false,
            MemberInfo declaring = null
        )
        {
            if (targets.Any(t => type.IsAssignableFrom(t)))
            {
                yield return path;
                yield break;
            }

            HashSet<Type> nonRecursiveTypes = new HashSet<Type> {
                typeof(sbyte),
                typeof(byte),
                typeof(short),
                typeof(ushort),
                typeof(int),
                typeof(uint),
                typeof(long),
                typeof(ulong),
                typeof(char),
                typeof(float),
                typeof(double),
                typeof(decimal),
                typeof(bool),
                typeof(string),
                typeof(Enum)
            };

            IEnumerable<MemberInfo> members = type.GetFields(BindingFlags.Public | BindingFlags.Instance).Cast<MemberInfo>()
                .Concat(type.GetProperties(BindingFlags.Public | BindingFlags.Instance));

            foreach (MemberInfo member in members)
            {
                ObsoleteAttribute obsoleteAttribute = member.GetCustomAttribute<ObsoleteAttribute>();

                Type memberType = (member as FieldInfo)?.FieldType ?? (member as PropertyInfo)?.PropertyType;
                bool hasSetMethod = (member is FieldInfo) ? true : (member as PropertyInfo)?.SetMethod != null;

                // if (path == "/transform/parent/root/parent")
                //     Debug.Log(excludePaths.Contains(path.Trim('/').Replace('/', '.')));

                if (
                    obsoleteAttribute != null ||
                    (needsSetter && member.DeclaringType.IsValueType) ||
                    member.Name == "Item" ||
                    excludePaths.Contains(path.Trim('/').Replace('/', '.')) ||
                    excludeTypes.Contains(memberType)
                )
                    continue;

                if (targets.Any(t => memberType.IsAssignableFrom(t)) && (!needsSetter || hasSetMethod))
                {
                    yield return $"{path}/{member.Name}{(showType ? $" ({memberType.Name})" : "")}";
                }
                else if (member.Name != declaring?.Name && !nonRecursiveTypes.Any(t => t.IsAssignableFrom(memberType)))
                {
                    foreach (string s in EnumerateProperties(memberType, targets, excludePaths, excludeTypes, path + "/" + member.Name, needsSetter, showType, member))
                        yield return s;
                }
            }
        }
    }
}