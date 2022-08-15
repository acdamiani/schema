using System;
using System.IO;
using System.Text;
using Schema.Utilities;
using UnityEditor;
using UnityEngine;

namespace SchemaEditor
{
    internal static class Styles
    {
        public static readonly Color lowerPriorityColor = new Color32(255, 140, 144, 255);
        public static readonly Color selfColor = new Color32(71, 255, 166, 255);
        private static readonly Color DarkBackgroundColor = new Color32(56, 56, 56, 255);
        private static readonly Color LightBackgroundColor = new Color32(200, 200, 200, 255);
        private static readonly Color DarkBorder = new Color32(40, 40, 40, 255);
        private static readonly Color LightBorder = new Color32(147, 147, 147, 255);

        private static GUIStyle _title;

        private static GUIStyle _window;

        private static GUIStyle _blackboardScroll;

        private static GUIStyle _favoriteToggle;

        private static GUIStyle _padding8x;

        private static GUIStyle _searchResult;

        private static GUIStyle _shadow;

        private static GUIStyle _roundedBox;

        private static GUIStyle _nodeLabel;

        private static GUIStyle _nodeIcon;

        private static GUIStyle _conditional;

        private static GUIStyle _element;

        private static GUIStyle _center;

        private static GUIStyle _searchLarge;

        private static GUIStyle _searchTopBar;

        private static GUIStyle _searchTopBarButton;

        private static GUIStyle _cancelButton;

        private static GUIStyle _blackboardEditorBackground;

        private static GUIStyle _blackboardEntry;

        private static GUIStyle _outline;

        public static Color windowBackground => EditorGUIUtility.isProSkin ? DarkBackgroundColor : LightBackgroundColor;
        public static Color windowAccent => EditorGUIUtility.isProSkin ? DarkBorder : LightBorder;

        public static Color outlineColor =>
            EditorGUIUtility.isProSkin ? new Color32(80, 80, 80, 255) : new Color32(176, 176, 176, 255);

        public static GUIStyle title
        {
            get
            {
                if (_title == null)
                {
                    _title = new GUIStyle(EditorStyles.label);
                    _title.alignment = TextAnchor.MiddleCenter;
                    _title.fontSize = 16;
                }

                return _title;
            }
        }

        public static GUIStyle window
        {
            get
            {
                if (_window == null)
                {
                    _window = new GUIStyle();
                    _window.normal.background = Icons.GetResource("QuickSearch/search_bg");
                    _window.border = new RectOffset(8, 8, 8, 8);
                    _window.padding = new RectOffset(2, 2, 2, 2);
                }

                return _window;
            }
        }

        public static GUIStyle blackboardScroll
        {
            get
            {
                if (_blackboardScroll == null)
                {
                    _blackboardScroll = new GUIStyle(EditorStyles.helpBox);
                    _blackboardScroll.padding = new RectOffset(0, 0, 0, 0);
                }

                return _blackboardScroll;
            }
        }

        public static GUIStyle favoriteToggle
        {
            get
            {
                if (_favoriteToggle == null)
                {
                    _favoriteToggle = new GUIStyle();
                    _favoriteToggle.margin = new RectOffset(0, 8, 0, 0);
                    _favoriteToggle.stretchHeight = false;
                    _favoriteToggle.stretchWidth = false;
                    _favoriteToggle.fixedHeight = 16;
                    _favoriteToggle.fixedWidth = 16;
                    _favoriteToggle.normal.background = Icons.GetResource("QuickSearch/favorite_disabled");
                    _favoriteToggle.onNormal.background = Icons.GetResource("QuickSearch/favorite_enabled");
                }

                return _favoriteToggle;
            }
        }

        public static GUIStyle padding8x
        {
            get
            {
                if (_padding8x == null)
                {
                    _padding8x = new GUIStyle();
                    _padding8x.padding = new RectOffset(8, 8, 8, 8);
                }

                return _padding8x;
            }
        }

        public static GUIStyle searchResult
        {
            get
            {
                if (_searchResult == null)
                {
                    _searchResult = new GUIStyle();
                    _searchResult.border = new RectOffset(8, 8, 8, 8);
                    _searchResult.padding = new RectOffset(4, 0, 0, 0);
                    _searchResult.alignment = TextAnchor.MiddleLeft;
                    _searchResult.hover.background =
                        roundedBox.normal.background.Tint(GUI.skin.settings.selectionColor);
                    _searchResult.hover.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
                    _searchResult.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
                }

                return _searchResult;
            }
        }

        public static GUIStyle shadow
        {
            get
            {
                if (_shadow == null)
                {
                    _shadow = new GUIStyle();
                    _shadow.border = new RectOffset(36, 36, 36, 36);
                    _shadow.normal.background = Resources.Load<Texture2D>("node");
                }

                return _shadow;
            }
        }

        public static GUIStyle roundedBox
        {
            get
            {
                if (_roundedBox == null)
                {
                    _roundedBox = new GUIStyle(EditorStyles.label);
                    _roundedBox.border = new RectOffset(8, 8, 8, 8);
                    _roundedBox.normal.background = Resources.Load<Texture2D>("round");
                    _roundedBox.alignment = TextAnchor.MiddleCenter;
                    _roundedBox.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
                }

                return _roundedBox;
            }
        }

        public static GUIStyle nodeLabel
        {
            get
            {
                if (_nodeLabel == null)
                {
                    _nodeLabel = new GUIStyle();
                    _nodeLabel.alignment = TextAnchor.MiddleCenter;
                    _nodeLabel.imagePosition = ImagePosition.TextOnly;
                    _nodeLabel.fontSize = 15;
                    _nodeLabel.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
                }

                return _nodeLabel;
            }
        }

        public static GUIStyle nodeIcon
        {
            get
            {
                if (_nodeIcon == null)
                {
                    _nodeIcon = new GUIStyle();
                    _nodeIcon.alignment = TextAnchor.MiddleCenter;
                    _nodeIcon.imagePosition = ImagePosition.ImageOnly;
                    _nodeIcon.fixedHeight = 64f;
                    _nodeIcon.fixedWidth = 64f;
                }

                return _nodeIcon;
            }
        }

        public static GUIStyle conditional
        {
            get
            {
                if (_conditional == null)
                {
                    _conditional = new GUIStyle();
                    _conditional.alignment = TextAnchor.MiddleRight;
                    _conditional.imagePosition = ImagePosition.TextOnly;
                    _conditional.fixedHeight = 32f;
                    _conditional.fontSize = 15;
                    _conditional.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
                    _conditional.padding = new RectOffset(8, 8, 8, 8);
                }

                return _conditional;
            }
        }

        public static GUIStyle element
        {
            get
            {
                if (_element == null)
                {
                    _element = new GUIStyle();
                    _element.normal.background = Icons.GetResource("element", false);
                    _element.border = new RectOffset(16, 16, 16, 16);
                }

                return _element;
            }
        }

        public static GUIStyle center
        {
            get
            {
                if (_center == null)
                {
                    _center = new GUIStyle(EditorStyles.label);
                    _center.alignment = TextAnchor.MiddleLeft;
                }

                return _center;
            }
        }

        public static GUIStyle searchLarge
        {
            get
            {
                if (_searchLarge == null)
                {
                    _searchLarge = new GUIStyle(EditorStyles.toolbarTextField);
                    _searchLarge.fixedHeight = 30f;
                    _searchLarge.stretchWidth = true;
                    _searchLarge.fontSize = 16;
                    _searchLarge.padding = new RectOffset(30, 0, 0, 0);
                }

                return _searchLarge;
            }
        }

        public static GUIStyle searchTopBar
        {
            get
            {
                if (_searchTopBar == null)
                {
                    _searchTopBar = new GUIStyle();
                    _searchTopBar.padding = new RectOffset(4, 4, 4, 4);
                    _searchTopBar.stretchWidth = true;
                }

                return _searchTopBar;
            }
        }

        public static GUIStyle searchTopBarButton
        {
            get
            {
                if (_searchTopBarButton == null)
                {
                    _searchTopBarButton = new GUIStyle(GUI.skin.button);
                    _searchTopBarButton.imagePosition = ImagePosition.ImageOnly;
                    _searchTopBarButton.padding = new RectOffset(4, 4, 4, 4);
                    _searchTopBarButton.fixedHeight = 24f;
                    _searchTopBarButton.fixedWidth = 24f;
                }

                return _searchTopBarButton;
            }
        }

        public static GUIStyle cancelButton
        {
            get
            {
                if (_cancelButton == null)
                {
                    _cancelButton = new GUIStyle();
                    _cancelButton.fixedHeight = 24f;
                    _cancelButton.fixedWidth = 24f;
                }

                return _cancelButton;
            }
        }

        public static GUIStyle blackboardEditorBackground
        {
            get
            {
                if (_blackboardEditorBackground == null)
                {
                    _blackboardEditorBackground = new GUIStyle(EditorStyles.helpBox);
                    _blackboardEditorBackground.padding = new RectOffset(1, 1, 8, 8);
                }

                return _blackboardEditorBackground;
            }
        }

        public static GUIStyle blackboardEntry
        {
            get
            {
                if (_blackboardEntry == null)
                {
                    Color textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;

                    _blackboardEntry = new GUIStyle();
                    _blackboardEntry.fixedHeight = 32f;
                    _blackboardEntry.stretchWidth = true;
                    _blackboardEntry.alignment = TextAnchor.MiddleLeft;
                    _blackboardEntry.padding = new RectOffset(8, 0, 0, 0);
                    _blackboardEntry.normal.textColor = textColor;
                    _blackboardEntry.hover.textColor = textColor;
                    _blackboardEntry.hover.background =
                        GenerateSolid(new Color(0.5f, 0.5f, 0.5f, 0.5f), Vector2Int.one);
                    _blackboardEntry.onNormal.textColor = textColor;
                    _blackboardEntry.onNormal.background =
                        GenerateSolid(GUI.skin.settings.selectionColor, Vector2Int.one);
                }

                return _blackboardEntry;
            }
        }

        public static GUIStyle outline
        {
            get
            {
                if (_outline == null)
                {
                    _outline = new GUIStyle();
                    _outline.normal.background = Icons.GetResource("outline", false);
                    _outline.border = new RectOffset(8, 8, 8, 8);
                }

                return _outline;
            }
        }

        private static Texture2D GenerateSolid(Color color, Vector2Int size)
        {
            Texture2D tex = new Texture2D(size.y, size.x);
            for (int y = 0; y < size.y; y++)
                for (int x = 0; x < size.x; x++)
                    tex.SetPixel(x, y, color);

            tex.wrapMode = TextureWrapMode.Repeat;
            tex.name = "Solid";

            tex.Apply();

            return tex;
        }
    }
}