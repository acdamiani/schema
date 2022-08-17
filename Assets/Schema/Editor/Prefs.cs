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
            List<TypeCode> valid = new() { TypeCode.String, TypeCode.Single, TypeCode.Boolean, TypeCode.Int32 };

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

    public class PrefsProvider : SettingsProvider
    {
        public PrefsProvider(string path, SettingsScope scope = SettingsScope.User)
            : base(path, scope)
        {
        }

        public override void OnGUI(string searchContext)
        {
            bool wideMode = EditorGUIUtility.wideMode;
            float labelWidth = EditorGUIUtility.labelWidth;

            EditorGUIUtility.wideMode = false;
            EditorGUIUtility.labelWidth = 200f;

            EditorGUILayout.LabelField("Editor", EditorStyles.boldLabel);
            Prefs.dimUnconnectedNodes = EditorGUILayout.Toggle(
                new GUIContent("Dim Unconnected Nodes", "Dim nodes not connected to the root node"),
                Prefs.dimUnconnectedNodes);
            Prefs.showGrid =
                EditorGUILayout.Toggle(new GUIContent("Show Grid", "Show the background grid in the editor"),
                    Prefs.showGrid);
            Prefs.minimapEnabled =
                EditorGUILayout.Toggle(new GUIContent("Show Minimap", "Show the minimap in the editor"),
                    Prefs.minimapEnabled);
            Prefs.gridSnap =
                EditorGUILayout.Toggle(new GUIContent("Grid Snap", "Snap nodes to a grid"), Prefs.gridSnap);
            Prefs.zoomSpeed = EditorGUILayout.Slider(new GUIContent("Zoom Speed", "The zoom speed in the editor"),
                Prefs.zoomSpeed, 0.01f, 0.1f);
            Prefs.arrangeHorizontalSpacing = EditorGUILayout.Slider(
                new GUIContent("Arrange Horizontal Spacing", "Horizontal spacing between nodes when arranged"),
                Prefs.arrangeHorizontalSpacing, 0f, 500f);
            Prefs.arrangeVerticalSpacing = EditorGUILayout.Slider(
                new GUIContent("Arrange Vertical Spacing", "Vertical spacing between ndoes when arranged"),
                Prefs.arrangeVerticalSpacing, 0f, 500f);
            Prefs.enableStatusIndicators = EditorGUILayout.Toggle(
                new GUIContent("Enable Status Indicators", "Toggle status indicators for all nodes"),
                Prefs.enableStatusIndicators
            );

            EditorGUILayout.LabelField("");

            EditorGUILayout.LabelField("Colors", EditorStyles.boldLabel);
            Prefs.selectionColor = EditorGUILayout.ColorField(
                new GUIContent("Selection Color", "The selection color to use for nodes"),
                Prefs.selectionColor
            );
            Prefs.highlightColor = EditorGUILayout.ColorField(
                new GUIContent("Highlight Color", "The color to use when highlighting a node"),
                Prefs.highlightColor
            );
            Prefs.successColor = EditorGUILayout.ColorField(
                new GUIContent("Success Color", "Color to use when successful"),
                Prefs.successColor
            );
            Prefs.failureColor = EditorGUILayout.ColorField(
                new GUIContent("Failure Color", "Color to use when failed"),
                Prefs.failureColor
            );
            Prefs.connectionColor = EditorGUILayout.ColorField(
                new GUIContent("Connection Color", "Color to use for node connections"),
                Prefs.connectionColor
            );
            Prefs.portColor = EditorGUILayout.ColorField(
                new GUIContent("Port Color", "Color to use for node ports"),
                Prefs.portColor
            );

            EditorGUILayout.LabelField("");

            EditorGUILayout.LabelField("Minimap", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Minimap Position");
            Prefs.minimapPosition = GUILayout.Toolbar(Prefs.minimapPosition,
                new[] { "Bottom Left", "Top Left", "Bottom Right", "Top Right" });
            Prefs.minimapWidth = EditorGUILayout.FloatField("Minimap Width", Prefs.minimapWidth);
            Prefs.maxMinimapHeight = EditorGUILayout.FloatField("Max Minimap Height", Prefs.maxMinimapHeight);
            Prefs.minimapOpacity = EditorGUILayout.Slider("Minimap Opacity", Prefs.minimapOpacity, 0f, 1f);
            Prefs.minimapOutlineColor = EditorGUILayout.ColorField("Minimap Outline Color", Prefs.minimapOutlineColor);

            EditorGUILayout.LabelField("");

            if (GUILayout.Button("Reset to default"))
                Prefs.ResetToDefault();

            EditorGUIUtility.wideMode = wideMode;
            EditorGUIUtility.labelWidth = labelWidth;
        }

        // Register the SettingsProvider
        [SettingsProvider]
        public static SettingsProvider CreateMyCustomSettingsProvider()
        {
            return new PrefsProvider("Preferences/Schema");
        }
    }
}