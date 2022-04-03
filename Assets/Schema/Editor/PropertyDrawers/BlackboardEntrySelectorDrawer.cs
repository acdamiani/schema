using UnityEngine;
using UnityEditor;
using Schema.Utilities;
using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;

[CustomPropertyDrawer(typeof(BlackboardEntrySelector), true)]
public class BlackboardEntrySelectorDrawer : PropertyDrawer
{
    public static Dictionary<string, string> names = new Dictionary<string, string>();
    private static Dictionary<SerializedProperty, bool> writeOnly = new Dictionary<SerializedProperty, bool>();
    private Vector2 scroll;
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty entryID = property.FindPropertyRelative("entryID");
        SerializedProperty entryName = property.FindPropertyRelative("entryName");
        SerializedProperty valuePathProp = property.FindPropertyRelative("valuePath");
        SerializedProperty value = property.FindPropertyRelative("_value");

        if (!writeOnly.ContainsKey(property))
            writeOnly[property] = fieldInfo.GetCustomAttribute<WriteOnlyAttribute>() != null;

        bool lastWideMode = EditorGUIUtility.wideMode;
        EditorGUIUtility.wideMode = true;

        Rect enumRect = new Rect(position.x, position.y, position.width - 19, position.height - 18);
        Rect textRect = new Rect(position.x, position.y + position.height - 18, position.width, 18);
        Rect buttonRect = new Rect(position.xMax - 19, position.y, 19, Mathf.Min(position.height, EditorGUIUtility.singleLineHeight));

        Vector2 oldIconSize = EditorGUIUtility.GetIconSize();
        EditorGUIUtility.SetIconSize(new Vector2(12, 12));

        EditorGUI.BeginProperty(position, label, property);

        bool doesHavePath = !String.IsNullOrEmpty(entryID.stringValue);

        if (!String.IsNullOrEmpty(entryID.stringValue) && !names.ContainsKey(entryID.stringValue))
            names[entryID.stringValue] = Blackboard.instance.GetEntry(entryID.stringValue).Name;

        if (value != null && !writeOnly[property])
        {
            EditorGUI.BeginDisabledGroup(doesHavePath);

            EditorGUI.PropertyField(enumRect, value, label, true);

            EditorGUI.EndDisabledGroup();
            // Hack to get context rect
            if (!String.IsNullOrEmpty(entryID.stringValue))
            {
                Rect p = EditorGUI.PrefixLabel(textRect, new GUIContent("\0"));
                Vector2 size = EditorStyles.miniLabel.CalcSize(new GUIContent($"Using {names[entryID.stringValue]}{valuePathProp.stringValue.Replace('/', '.')}"));
                GUI.BeginClip(p, scroll, Vector2.zero, false);
                GUI.Label(new Rect(0f, 0f, size.x, 20f), $"Using {names[entryID.stringValue]}{valuePathProp.stringValue.Replace('/', '.')}", EditorStyles.miniLabel);
                GUI.EndClip();

                if (size.x > p.width && p.Contains(Event.current.mousePosition) && Event.current.type == EventType.ScrollWheel)
                {
                    scroll = new Vector2(Mathf.Clamp(scroll.x + Event.current.delta.y * 4, -(size.x - p.width), 0f), 0f);
                }
            }

            GUIContent c = EditorGUIUtility.ObjectContent(null, typeof(Transform));

            GUIStyle t = new GUIStyle("ObjectFieldButton");
            if (property.propertyPath.EndsWith("]"))
                t.fixedHeight = Mathf.Min(position.height, EditorGUIUtility.singleLineHeight) - 2;
            else
                t.fixedHeight = Mathf.Min(position.height, EditorGUIUtility.singleLineHeight);

            t.margin.Remove(buttonRect);
            t.normal.textColor = Color.white;

            GUI.Label(buttonRect, "", t);

            if (Event.current.type == EventType.MouseDown && buttonRect.Contains(Event.current.mousePosition))
            {
                GenericMenu menu = GenerateMenu(property);

                menu.DropDown(buttonRect);
            }
        }
        else
        {
            Rect controlRect = EditorGUI.PrefixLabel(position, label);

            string path = valuePathProp.stringValue;
            names.TryGetValue(entryID.stringValue, out string idName);

            GUIContent buttonValue = new GUIContent(String.IsNullOrEmpty(idName) ? "None" : idName);

            if (EditorGUI.DropdownButton(controlRect, buttonValue, FocusType.Passive))
            {
                GenericMenu menu = GenerateMenu(property);

                menu.DropDown(controlRect);
            }
        }

        EditorGUI.EndProperty();

        EditorGUIUtility.SetIconSize(oldIconSize);

        EditorGUIUtility.wideMode = lastWideMode;
    }
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        SerializedProperty valueProp = property.FindPropertyRelative("_value");

        bool lastWideMode = EditorGUIUtility.wideMode;
        EditorGUIUtility.wideMode = true;

        float height;

        if (!writeOnly.ContainsKey(property))
            writeOnly[property] = fieldInfo.GetCustomAttribute<WriteOnlyAttribute>() != null;

        if (valueProp != null && !writeOnly[property])
            height = EditorGUI.GetPropertyHeight(valueProp, label, true) + 18;
        else
            height = base.GetPropertyHeight(property, label);

        EditorGUIUtility.wideMode = lastWideMode;

        return height;
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

    private GenericMenu GenerateMenu(SerializedProperty property)
    {
        SerializedProperty idProp = property.FindPropertyRelative("entryID");
        SerializedProperty valuePathProp = property.FindPropertyRelative("valuePath");
        SerializedProperty typeMask = property.FindPropertyRelative("blackboardTypesMask");

        GenericMenu menu = new GenericMenu();

        if (typeMask.intValue == -1)
        {
            List<string> filtersList = new List<string>();

            SerializedProperty filters = property.FindPropertyRelative("filters");

            for (int i = 0; i < filters.arraySize; i++)
            {
                filtersList.Add(filters.GetArrayElementAtIndex(i).stringValue);
            }

            typeMask.intValue = Blackboard.instance.GetMask(filtersList).Item2;
        }

        menu.AddItem("None", String.IsNullOrEmpty(idProp.stringValue), () => GenericMenuSelectOption(property, ""), false);

        List<Type> filtered = HelperMethods.FilterArrayByMask(Blackboard.typeColors.Keys.Reverse().ToArray(), typeMask.intValue).ToList();

        foreach (string s in Blackboard.instance.entryByteStrings)
        {
            string entryID = GetID(s);
            BlackboardEntry entry = Blackboard.instance.GetEntry(entryID);

            IEnumerable<string> props = null;

            if (fieldInfo.GetCustomAttribute<DisableDynamicBindingAttribute>() != null)
            {
                if (!filtered.Contains(entry.type))
                    continue;

                menu.AddItem(entry.Name, idProp.stringValue == entryID, () => GenericMenuSelectOption(property, entryID, entry.type, "/"), false);
            }
            else
            {
                props = PrintProperties(
                    entry.type,
                    entry.type,
                    filtered,
                    ""
                );

                foreach (string ss in props)
                {
                    menu.AddItem(
                        entry.Name + ss,
                        valuePathProp.stringValue.Equals(ss) && idProp.stringValue == entryID,
                        () => GenericMenuSelectOption(property, entryID, entry.type, ss),
                        false
                    );
                }
            }
        }

        return menu;
    }
    private void GenericMenuSelectOption(SerializedProperty property, string id, Type type = null, string path = "")
    {
        SerializedProperty idProperty = property.FindPropertyRelative("entryID");
        SerializedProperty entryTypeProperty = property.FindPropertyRelative("entryTypeString");
        SerializedProperty valuePathProperty = property.FindPropertyRelative("valuePath");

        idProperty.stringValue = id;
        valuePathProperty.stringValue = path;

        if (type != null)
            entryTypeProperty.stringValue = type.AssemblyQualifiedName;

        property.serializedObject.ApplyModifiedProperties();
    }
    private IEnumerable<string> PrintProperties(Type baseType, Type type, List<Type> targets, string basePath)
    {
        if (targets.Contains(type))
        {
            yield return basePath;
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

        foreach (PropertyInfo field in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            ObsoleteAttribute obsoleteAttribute = field.GetCustomAttribute<ObsoleteAttribute>();

            if (obsoleteAttribute != null)
                continue;

            if (field.Name == "Item")
                continue;

            if (targets.Contains(field.PropertyType))
            {
                yield return basePath + "/" + field.Name;
            }
            else if (field.PropertyType != type && field.PropertyType != baseType && !nonRecursiveTypes.Any(t => t.IsAssignableFrom(field.PropertyType)))
            {
                foreach (string s in PrintProperties(baseType, field.PropertyType, targets, basePath + "/" + field.Name))
                    yield return s;
            }
        }

        foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            ObsoleteAttribute obsoleteAttribute = field.GetCustomAttribute<ObsoleteAttribute>();

            if (obsoleteAttribute != null)
                continue;

            if (targets.Contains(field.FieldType))
            {
                yield return basePath + "/" + field.Name;
            }
            else if (field.FieldType != type && field.FieldType != baseType && !nonRecursiveTypes.Any(t => t.IsAssignableFrom(field.FieldType)))
            {
                foreach (string s in PrintProperties(baseType, field.FieldType, targets, basePath + "/" + field.Name))
                    yield return s;
            }
        }
    }
}