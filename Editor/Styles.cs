using UnityEditor;
using UnityEngine;

namespace SchemaEditor
{
    internal static class Styles
    {
        private static readonly Color DarkBackgroundColor = new Color32(56, 56, 56, 255);
        private static readonly Color LightBackgroundColor = new Color32(200, 200, 200, 255);
        private static readonly Color DarkBorder = new Color32(40, 40, 40, 255);
        private static readonly Color LightBorder = new Color32(147, 147, 147, 255);

        private static GUIStyle _window;
        private static GUIStyle _blackboardScroll;
        private static GUIStyle _favoriteToggle;
        private static GUIStyle _padding8X;
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
        private static GUIStyle _description;
        private static GUIStyle _selectorDrawerMiniText;
        private static GUIStyle _priorityIndicator;
        public static Color WindowBackground => EditorGUIUtility.isProSkin ? DarkBackgroundColor : LightBackgroundColor;
        public static Color WindowAccent => EditorGUIUtility.isProSkin ? DarkBorder : LightBorder;
        public static Color? WindowBorder => EditorGUIUtility.isProSkin ? new Color32(80, 80, 80, 255) : (Color?)null;

        public static Color OutlineColor =>
            EditorGUIUtility.isProSkin ? new Color32(80, 80, 80, 255) : new Color32(176, 176, 176, 255);

        public static GUIStyle PriorityIndicator
        {
            get
            {
                if (_priorityIndicator == null)
                {
                    _priorityIndicator = new GUIStyle(EditorStyles.label);
                    _priorityIndicator.alignment = TextAnchor.MiddleCenter;
                    _priorityIndicator.fontSize = 12;
                    _priorityIndicator.fontStyle = FontStyle.Bold;
                    _priorityIndicator.normal.textColor = Color.white;
                }

                return _priorityIndicator;
            }
        }

        public static GUIStyle Window
        {
            get
            {
                if (_window == null)
                {
                    _window = new GUIStyle();
                    _window.padding = new RectOffset(2, 2, 2, 2);
                }

                return _window;
            }
        }

        public static GUIStyle FavoriteToggle
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

        public static GUIStyle Padding8X
        {
            get
            {
                if (_padding8X == null)
                {
                    _padding8X = new GUIStyle();
                    _padding8X.padding = new RectOffset(8, 8, 8, 8);
                }

                return _padding8X;
            }
        }

        public static GUIStyle SearchResult
        {
            get
            {
                if (_searchResult == null)
                {
                    _searchResult = new GUIStyle();
                    _searchResult.padding = new RectOffset(4, 0, 0, 0);
                    _searchResult.alignment = TextAnchor.MiddleLeft;
                    _searchResult.hover.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
                    _searchResult.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
                }

                return _searchResult;
            }
        }

        public static GUIStyle Shadow
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

        public static GUIStyle RoundedBox
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

        public static GUIStyle NodeLabel
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

        public static GUIStyle NodeIcon
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

        public static GUIStyle Conditional
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

        public static GUIStyle Element
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

        public static GUIStyle Center
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

        public static GUIStyle SearchLarge
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

        public static GUIStyle SearchTopBar
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

        public static GUIStyle SearchTopBarButton
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

        public static GUIStyle CancelButton
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

        public static GUIStyle BlackboardEditorBackground
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

        public static GUIStyle BlackboardEntry
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

        public static GUIStyle Outline
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

        public static GUIStyle Description
        {
            get
            {
                if (_description == null)
                {
                    _description = new GUIStyle();
                    _description.wordWrap = true;
                    _description.fontSize = 14;
                    _description.normal.textColor = Color.white;
                }

                return _description;
            }
        }

        public static GUIStyle SelectorDrawerMiniText
        {
            get
            {
                if (_selectorDrawerMiniText == null)
                {
                    _selectorDrawerMiniText = new GUIStyle(EditorStyles.miniLabel);
                    _selectorDrawerMiniText.padding = new RectOffset(2, 2, 0, 0);
                }

                return _selectorDrawerMiniText;
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