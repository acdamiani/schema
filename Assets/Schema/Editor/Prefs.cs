using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace SchemaEditor.Internal
{
    public static class Prefs
    {
        public static bool dimUnconnectedNodes
        {
            get => EditorPrefs.GetBool("SCHEMA_PREF__dimUnconnectedNodes", true);
            set => EditorPrefs.SetBool("SCHEMA_PREF__dimUnconnectedNodes", value);
        }

        public static bool showGrid
        {
            get => EditorPrefs.GetBool("SCHEMA_PREF__showGrid", true);
            set => EditorPrefs.SetBool("SCHEMA_PREF__showGrid", value);
        }

        public static float zoomSpeed
        {
            get => EditorPrefs.GetFloat("SCHEMA_PREF__zoomSpeed", 0.035f);
            set => EditorPrefs.SetFloat("SCHEMA_PREF__zoomSpeed", value);
        }

        public static float arrangeHorizontalSpacing
        {
            get => EditorPrefs.GetFloat("SCHEMA_PREF__arrangeHorizontalSpacing", 25f);
            set => EditorPrefs.SetFloat("SCHEMA_PREF__arrangeHorizontalSpacing", value);
        }

        public static float arrangeVerticalSpacing
        {
            get => EditorPrefs.GetFloat("SCHEMA_PREF__arrangeVerticalSpacing", 100f);
            set => EditorPrefs.SetFloat("SCHEMA_PREF__arrangeVerticalSpacing", value);
        }

        public static Color selectionColor
        {
            get => GetColor("SCHEMA_PREF__selectionColor", Color.white);
            set => SetColor("SCHEMA_PREF__selectionColor", value);
        }

        public static Color highlightColor
        {
            get => GetColor("SCHEMA_PREF__highlightColor", new Color32(247, 181, 0, 255));
            set => SetColor("SCHEMA_PREF__highlightColor", value);
        }

        public static bool enableStatusIndicators
        {
            get => EditorPrefs.GetBool("SCHEMA_PREF__enableStatusIndicators", true);
            set => EditorPrefs.SetBool("SCHEMA_PREF__enableStatusIndicators", value);
        }

        public static Color successColor
        {
            get => GetColor("SCHEMA_PREF__successColor", new Color32(64, 255, 64, 255));
            set => SetColor("SCHEMA_PREF__successColor", value);
        }

        public static Color failureColor
        {
            get => GetColor("SCHEMA_PREF__failureColor", new Color32(255, 64, 64, 255));
            set => SetColor("SCHEMA_PREF__failureColor", value);
        }

        public static float minimapWidth
        {
            get => EditorPrefs.GetFloat("SCHEMA_PREF__minimapWidth", 250f);
            set => EditorPrefs.SetFloat("SCHEMA_PREF__minimapWidth", Mathf.Clamp(value, 100f, float.MaxValue));
        }

        public static float maxMinimapHeight
        {
            get => EditorPrefs.GetFloat("SCHEMA_PREF__maxMinimapHeight", 250f);
            set => EditorPrefs.SetFloat("SCHEMA_PREF__maxMinimapHeight", Mathf.Clamp(value, 100f, float.MaxValue));
        }

        public static int minimapPosition
        {
            get => EditorPrefs.GetInt("SCHEMA_PREF__minimapPosition", 2);
            set => EditorPrefs.SetInt("SCHEMA_PREF__minimapPosition", Mathf.Clamp(value, 0, 4));
        }

        public static float minimapOpacity
        {
            get => EditorPrefs.GetFloat("SCHEMA_PREF__minimapOpacity", 0.5f);
            set => EditorPrefs.SetFloat("SCHEMA_PREF__minimapOpacity", Mathf.Clamp01(value));
        }

        public static Color minimapOutlineColor
        {
            get => GetColor("SCHEMA_PREF__minimapOutlineColor", Color.gray);
            set => SetColor("SCHEMA_PREF__minimapOutlineColor", value);
        }

        public static bool enableDebugView
        {
            get => EditorPrefs.GetBool("SCHEMA_PREF__enableDebugView", false);
            set => EditorPrefs.SetBool("SCHEMA_PREF__enableDebugView", value);
        }

        public static bool enableDebugViewPlus
        {
            get => EditorPrefs.GetBool("SCHEMA_PREF__enableDebugViewPlus", false);
            set => EditorPrefs.SetBool("SCHEMA_PREF__enableDebugViewPlus", value);
        }

        public static Color connectionColor
        {
            get => GetColor("SCHEMA_PREF__connectionColor", Color.white);
            set => SetColor("SCHEMA_PREF__connectionColor", value);
        }

        public static Color portColor
        {
            get => GetColor("SCHEMA_PREF__portColor", new Color32(80, 80, 80, 255));
            set => SetColor("SCHEMA_PREF__portColor", value);
        }

        public static bool gridSnap
        {
            get => EditorPrefs.GetBool("SCHEMA_PREF__gridSnap", false);
            set => EditorPrefs.SetBool("SCHEMA_PREF__gridSnap", value);
        }

        public static bool minimapEnabled
        {
            get => EditorPrefs.GetBool("SCHEMA_PREF__minimapEnabled", false);
            set => EditorPrefs.SetBool("SCHEMA_PREF__minimapEnabled", value);
        }

        public static Color GetColor(string key, Color defaultValue)
        {
            float r = EditorPrefs.GetFloat(key + "_r", defaultValue.r);
            float g = EditorPrefs.GetFloat(key + "_g", defaultValue.g);
            float b = EditorPrefs.GetFloat(key + "_b", defaultValue.b);
            float a = EditorPrefs.GetFloat(key + "_a", defaultValue.a);

            return new Color(r, g, b, a);
        }

        public static void SetColor(string key, Color value)
        {
            EditorPrefs.SetFloat(key + "_r", value.r);
            EditorPrefs.SetFloat(key + "_g", value.g);
            EditorPrefs.SetFloat(key + "_b", value.b);
            EditorPrefs.SetFloat(key + "_a", value.a);
        }

        public static IEnumerable<string> GetList(string key)
        {
            string s = EditorPrefs.GetString(key, "");

            return s.Split(',');
        }

        public static void SetList(string key, IEnumerable<string> values)
        {
            EditorPrefs.SetString(key, string.Join(",", values));
        }

        public static void ResetToDefault()
        {
            List<TypeCode> valid = new List<TypeCode>
                { TypeCode.String, TypeCode.Single, TypeCode.Boolean, TypeCode.Int32 };

            Type t = typeof(Prefs);
            PropertyInfo[] fields = t.GetProperties(BindingFlags.Static | BindingFlags.Public);

            foreach (PropertyInfo property in fields)
                if (valid.Contains(Type.GetTypeCode(property.PropertyType)))
                {
                    EditorPrefs.DeleteKey("SCHEMA_PREF__" + property.Name);
                }
                else if (typeof(Color).IsAssignableFrom(property.PropertyType))
                {
                    EditorPrefs.DeleteKey("SCHEMA_PREF__" + property.Name + "_r");
                    EditorPrefs.DeleteKey("SCHEMA_PREF__" + property.Name + "_g");
                    EditorPrefs.DeleteKey("SCHEMA_PREF__" + property.Name + "_b");
                    EditorPrefs.DeleteKey("SCHEMA_PREF__" + property.Name + "_a");
                }
        }
    }

}