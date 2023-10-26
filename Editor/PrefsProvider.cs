using UnityEditor;
using UnityEngine;

namespace SchemaEditor.Internal
{
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