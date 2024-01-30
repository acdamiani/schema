using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Schema;
using Schema.Internal;
using Schema.Utilities;
using SchemaEditor.Utilities;
using UnityEditor;
using UnityEngine;

namespace SchemaEditor
{
    [CustomPropertyDrawer(typeof(BlackboardEntrySelector), true)]
    public class BlackboardEntrySelectorDrawer : PropertyDrawer
    {
        private static readonly Dictionary<string, SelectorPropertyInfo> info =
            new Dictionary<string, SelectorPropertyInfo>();

        private static readonly Type[] valid = { typeof(Node), typeof(Conditional), typeof(Modifier) };
        private static readonly CacheDictionary<Type, Type> typeMappings = new CacheDictionary<Type, Type>();

        private static readonly Dictionary<Type, Tuple<string[], Type[]>> excluded =
            new Dictionary<Type, Tuple<string[], Type[]>>();

        private static event GUIDelayCall guiDelayCall;

        public static void DoSelectorMenu(Rect position, SerializedProperty property, FieldInfo fieldInfo)
        {
            if (GUI.Button(position, Icons.GetEditor("_Menu"), EditorStyles.miniButtonRight))
            {
                GenericMenu menu = GenerateMenu(property, fieldInfo);

                menu.DropDown(position);
            }
        }

        public static string DoSelectorDrawer(Rect position, SerializedProperty property, GUIContent label,
            Type fieldType, FieldInfo fieldInfo)
        {
            Type parentType = property.serializedObject.targetObject.GetType();

            if (!valid.Any(t => t.IsAssignableFrom(parentType)))
            {
                GUI.Label(position,
                    new GUIContent("Cannot use a BlackboardEntrySelector in a non tree type",
                        Icons.GetEditor("console.warnicon")), EditorStyles.miniLabel);
                return "";
            }

            if (Blackboard.instance == null)
            {
                GUI.Label(position,
                    new GUIContent("Cannot use a BlackboardEntrySelector outside a tree",
                        Icons.GetEditor("console.warnicon")), EditorStyles.miniLabel);
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

            Vector2 size = EditorStyles.miniButtonRight.CalcSize(new GUIContent(Icons.GetEditor("_Menu")));

            Rect enumRect = new Rect(position.x, position.y, position.width - size.x,
                Mathf.Min(position.height, EditorGUIUtility.singleLineHeight));
            Rect textRect = new Rect(position.x, position.y + enumRect.height, position.width, enumRect.height);
            Rect buttonRect = new Rect(position.xMax - size.x, position.y, size.x,
                Mathf.Min(position.height, EditorGUIUtility.singleLineHeight));

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

                if (GUI.Button(buttonRect, Icons.GetEditor("_Menu"), EditorStyles.miniButtonRight))
                {
                    GenericMenu menu = GenerateMenu(property, fieldInfo);

                    menu.DropDown(buttonRect);
                }
            }
            else if (value != null && !info[property.propertyPath].writeOnly)
            {
                Rect r = EditorGUI.PrefixLabel(enumRect, label);

                EditorGUI.BeginDisabledGroup(doesHavePath);

                EditorGUI.PropertyField(r, value, GUIContent.none, true);

                EditorGUI.EndDisabledGroup();

                if (doesHavePath)
                {
                    r = new Rect(r.x, textRect.y, r.width + size.x, r.height);
                    r.y += 3f;

                    GUIContent content =
                        new GUIContent($"Using {entryValue.name}{valuePathProp.stringValue.Replace('/', '.')}");
                    size = Styles.SelectorDrawerMiniText.CalcSize(content);

                    GUI.BeginClip(r, new Vector2(info[property.propertyPath].scroll, 0f), Vector2.zero, false);

                    EditorGUI.LabelField(new Rect(0f, 3f, size.x, size.y), content, Styles.SelectorDrawerMiniText);

                    GUI.EndClip();

                    GUI.Box(r, GUIContent.none, EditorStyles.helpBox);

                    if (size.x > r.width && r.Contains(Event.current.mousePosition) &&
                        Event.current.type == EventType.ScrollWheel)
                    {
                        info[property.propertyPath].scroll = Mathf.Clamp(
                            info[property.propertyPath].scroll - Event.current.delta.y * 10, -(size.x - r.width), 0f);
                        // Prevent scroll
                        Event.current.delta = Vector2.zero;
                    }
                }

                if (GUI.Button(buttonRect, Icons.GetEditor("_Menu"), EditorStyles.miniButtonRight))
                {
                    GenericMenu menu = GenerateMenu(property, fieldInfo);

                    menu.DropDown(buttonRect);
                }
            }
            else
            {
                Rect controlRect = EditorGUI.PrefixLabel(position, label);

                string path = valuePathProp.stringValue;

                GUIContent buttonValue = new GUIContent(entryValue == null
                    ? "None"
                    : entryValue.name + valuePathProp.stringValue.Replace('/', '.').TrimEnd('.'));

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

            string entryName = entry.objectReferenceValue?.name +
                               valuePathProp.stringValue.TrimEnd('/').Replace('/', '.');

            if (!string.IsNullOrEmpty(entryName))
                return entryName;

            if (isDynamicProperty.boolValue)
                return "dynamic";
            if (value != null)
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
                height = EditorGUI.GetPropertyHeight(valueProp, label, true) +
                         (entry.objectReferenceValue != null ? EditorGUIUtility.singleLineHeight + 4 : 0);
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

            return Encoding.ASCII.GetString(idBytes);
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

            List<Type> filtered = HelperMethods
                .FilterArrayByMask(Blackboard.mappedBlackboardTypes.Reverse().ToArray(), mask)
                .ToList();

            menu.AddItem("<None>", entry.objectReferenceValue == null && !isDynamicPropertyValue,
                () => GenericMenuSelectOption(property, null), false);
            menu.AddSeparator("");
            menu.AddItem("<Dynamic>", isDynamicPropertyValue, () => ToggleDynamic(property), false);

            bool disableDynamicBinding = fieldInfo.GetCustomAttribute<DisableDynamicBindingAttribute>() != null;

            Dictionary<Type, IEnumerable<EnumeratedProperty>> tmp =
                new Dictionary<Type, IEnumerable<EnumeratedProperty>>();

            foreach (BlackboardEntry bEntry in Blackboard.instance.entries.Concat(Blackboard.global.entries))
            {
                Type value = typeMappings.GetOrCreate(bEntry.type, () => EntryType.GetMappedType(bEntry.type));
                string entryName = bEntry.blackboard == Blackboard.global ? $"(Global) {bEntry.name}" : bEntry.name;

                if (disableDynamicBinding)
                {
                    if (!filtered.Any(t => t.CanConvertTo(value)))
                        continue;

                    menu.AddItem(
                        entryName + (filtered.Count > 1 ? " (" + bEntry.type.Name + ")" : ""),
                        entry.objectReferenceValue == bEntry,
                        () => GenericMenuSelectOption(property, bEntry, path: "/"),
                        false
                    );
                }
                else if (filtered.Any(t => t.CanConvertTo(value)))
                {
                    bool showType = filtered.Count > 1;
                    string typeSuffix = $" ({bEntry.type.Name})";

                    menu.AddItem(
                        $"{entryName}{(showType ? typeSuffix : "")}",
                        entry.objectReferenceValue == bEntry,
                        () => GenericMenuSelectOption(property, bEntry),
                        false
                    );
                }
                else
                {
                    excluded.TryGetValue(bEntry.type, out Tuple<string[], Type[]> e);

                    e ??= excluded[bEntry.type] = new Tuple<string[], Type[]>(EntryType.GetExcludedPaths(bEntry.type),
                        EntryType.GetExcludedTypes(bEntry.type));

                    tmp.TryGetValue(bEntry.type, out IEnumerable<EnumeratedProperty> enumerated);

                    enumerated ??= tmp[bEntry.type] = EnumerateProperties(
                        value,
                        filtered,
                        e.Item1,
                        e.Item2,
                        needsSetter: info.ContainsKey(property.propertyPath) &&
                                     info[property.propertyPath].writeOnly,
                        showType: filtered.Count > 1
                    );

                    foreach (EnumeratedProperty p in enumerated)
                        menu.AddItem(
                            entryName + p.MenuPath,
                            valuePathProp.stringValue.Equals(p.MenuPath) && entry.objectReferenceValue == bEntry,
                            () => GenericMenuSelectOption(property, bEntry, p.Type, p.MenuPath),
                            false
                        );
                }
            }

            return menu;
        }


        private static void GenericMenuSelectOption(SerializedProperty property, BlackboardEntry entry,
            Type entryType = null, string path = "")
        {
            EntryType.TryGetMappedType(entryType ?? entry.type, out Type mappedEntryType);

            SerializedProperty entryProp = property.FindPropertyRelative("m_entry");
            SerializedProperty typeString = property.FindPropertyRelative("m_valueTypeString");
            SerializedProperty valuePathProperty = property.FindPropertyRelative("m_valuePath");
            SerializedProperty isDynamicProperty = property.FindPropertyRelative("m_isDynamic");

            entryProp.objectReferenceValue = entry;
            typeString.stringValue = mappedEntryType.AssemblyQualifiedName;
            valuePathProperty.stringValue = path;
            isDynamicProperty.boolValue = false;

            guiDelayCall += UpdateChanged;

            property.serializedObject.ApplyModifiedProperties();
        }

        private static void ToggleDynamic(SerializedProperty property)
        {
            SerializedProperty entryProp = property.FindPropertyRelative("m_entry");
            SerializedProperty typeString = property.FindPropertyRelative("m_valueTypeString");
            SerializedProperty valuePathProperty = property.FindPropertyRelative("m_valuePath");
            SerializedProperty isDynamicProperty = property.FindPropertyRelative("m_isDynamic");

            entryProp.objectReferenceValue = null;
            typeString.stringValue = "";
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

        private static IEnumerable<EnumeratedProperty> EnumerateProperties(
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
            HashSet<Type> nonRecursiveTypes = new HashSet<Type>
            {
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

            IEnumerable<MemberInfo> members = type.GetFields(BindingFlags.Public | BindingFlags.Instance)
                .Cast<MemberInfo>()
                .Concat(type.GetProperties(BindingFlags.Public | BindingFlags.Instance));

            IEnumerable<Type> targetsArray = targets as Type[] ?? targets.ToArray();
            IEnumerable<string> excludePathsArray = excludePaths as string[] ?? excludePaths.ToArray();
            IEnumerable<Type> excludeTypesArray = excludeTypes as Type[] ?? excludeTypes.ToArray();

            foreach (MemberInfo member in members)
            {
                ObsoleteAttribute obsoleteAttribute = member.GetCustomAttribute<ObsoleteAttribute>();

                Type memberType = (member as FieldInfo)?.FieldType ?? (member as PropertyInfo)?.PropertyType;
                bool hasSetMethod = member is FieldInfo || (member as PropertyInfo)?.GetSetMethod(false) != null;

                if (
                    obsoleteAttribute != null ||
                    (needsSetter && member.DeclaringType.IsValueType) ||
                    member.Name == "Item" ||
                    excludePathsArray.Contains((path + "/" + member.Name).Trim('/').Replace('/', '.')) ||
                    excludeTypesArray.Contains(memberType)
                )
                    continue;

                if (targetsArray.Any(t => t.IsAssignableFrom(memberType)) && (!needsSetter || hasSetMethod))
                    yield return new EnumeratedProperty(
                        $"{path}/{member.Name}{(showType ? $" ({memberType.Name})" : "")}",
                        memberType);
                else if (member.Name != declaring?.Name && !nonRecursiveTypes.Any(t => t.IsAssignableFrom(memberType)))
                    foreach (EnumeratedProperty s in EnumerateProperties(memberType, targetsArray, excludePathsArray,
                                 excludeTypesArray,
                                 path + "/" + member.Name, needsSetter, showType, member))
                        yield return s;
            }
        }

        private class EnumeratedProperty
        {
            public EnumeratedProperty(string menuPath, Type type)
            {
                MenuPath = menuPath;
                Type = type;
            }

            public string MenuPath { get; }
            public Type Type { get; }
        }

        private delegate void GUIDelayCall(SerializedProperty property);

        private class SelectorPropertyInfo
        {
            public float scroll;
            public bool writeOnly;
        }
    }
}