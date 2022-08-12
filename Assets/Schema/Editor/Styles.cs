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
        private static Texture2D _gridTexture;

        private static Texture2D _gridTexture2x;

        private static Texture2D _favoriteDisabled;

        private static Texture2D _favoriteEnabled;

        private static Texture2D _folder;

        private static Texture2D _folderOpen;

        private static Texture2D _next;
        private static Texture2D _prev;
        private static Texture2D _menu;
        private static Texture2D _inspectorIcon;

        private static Texture2D _hiearchyIcon;

        private static Texture2D _searchBackground;

        private static Texture2D _solid;
        private static Texture2D _curve;
        private static Texture2D _inConnectionOutline;

        private static Texture2D _foldout;

        private static Texture2D _moveUp;
        private static Texture2D _moveDown;

        private static Texture2D _close;
        private static GUIStyle _title;

        private static GUIStyle _quickSearch;

        private static GUIStyle _blackboardScroll;

        private static GUIStyle _favoriteToggle;

        private static GUIStyle _padding8x;

        private static GUIStyle _searchResult;

        private static GUIStyle _shadow;

        private static GUIStyle _roundedBox;

        private static GUIStyle _nodeLabel;

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

        private static readonly string _megamind =
            "4oCU4oCU4oCU4oCU4oCU4oCU4oCU4oCU4oCUTm8gZW50cmllcz/igJTigJTigJTigJTigJTigJTigJTigJTigJQK4qCA4qOe4qK94qKq4qKj4qKj4qKj4qKr4qG64qG14qOd4qGu4qOX4qK34qK94qK94qK94qOu4qG34qG94qOc4qOc4qKu4qK64qOc4qK34qK94qKd4qG94qOdCuKguOKhuOKgnOKgleKgleKggeKigeKih+Kij+KiveKiuuKjquKhs+KhneKjjuKjj+Kir+KinuKhv+Kjn+Kjt+Kjs+Kir+Kht+KjveKiveKir+Kjs+Kjq+KghwrioIDioIDiooDiooDiooTioqzioqrioarioY7io4bioYjioJrioJzioJXioIfioJfioJ3iopXioq/ioqvio57io6/io7/io7viob3io4/iopfio5fioI/ioIAK4qCA4qCq4qGq4qGq4qOq4qKq4qK64qK44qKi4qKT4qKG4qKk4qKA4qCA4qCA4qCA4qCA4qCI4qKK4qKe4qG+4qO/4qGv4qOP4qKu4qC34qCB4qCA4qCACuKggOKggOKggOKgiOKgiuKghuKhg+KgleKileKih+Kih+Kih+Kih+Kih+Kij+KijuKijuKihuKihOKggOKikeKjveKjv+KineKgsuKgieKggOKggOKggOKggArioIDioIDioIDioIDioIDiob/ioILioKDioIDioYfioofioJXioojio4DioIDioIHioKHioKPioaPioavio4Lio7/ioK/ioqrioLDioILioIDioIDioIDioIAK4qCA4qCA4qCA4qCA4qGm4qGZ4qGC4qKA4qKk4qKj4qCj4qGI4qO+4qGD4qCg4qCE4qCA4qGE4qKx4qOM4qO24qKP4qKK4qCC4qCA4qCA4qCA4qCA4qCA4qCACuKggOKggOKggOKggOKineKhsuKjnOKhruKhj+KijuKijOKiguKgmeKgouKgkOKigOKimOKiteKjveKjv+Khv+KggeKggeKggOKggOKggOKggOKggOKggOKggArioIDioIDioIDioIDioKjio7riobrioZXioZXiobHioZHioYbioZXioYXioZXioZziobzior3iobvioI/ioIDioIDioIDioIDioIDioIDioIDioIDioIDioIAK4qCA4qCA4qCA4qCA4qO84qOz4qOr4qO+4qO14qOX4qG14qGx4qGh4qKj4qKR4qKV4qKc4qKV4qGd4qCA4qCA4qCA4qCA4qCA4qCA4qCA4qCA4qCA4qCA4qCACuKggOKggOKggOKjtOKjv+KjvuKjv+Kjv+Kjv+Khv+KhveKhkeKijOKgquKhouKho+Kjo+Khn+KggOKggOKggOKggOKggOKggOKggOKggOKggOKggOKggOKggArioIDioIDioIDioZ/iob7io7/ior/ior/iorXio73io77io7zio5jiorjiorjio57ioZ/ioIDioIDioIDioIDioIDioIDioIDioIDioIDioIDioIDioIDioIAK4qCA4qCA4qCA4qCA4qCB4qCH4qCh4qCp4qGr4qK/4qOd4qG74qGu4qOS4qK94qCL4qCA4qCA4qCA4qCA4qCA4qCA4qCA4qCA4qCA4qCA4qCA4qCA4qCA4qCACuKAlOKAlOKAlOKAlOKAlOKAlOKAlOKAlOKAlOKAlOKAlOKAlOKAlOKAlOKAlOKAlOKAlOKAlOKAlOKAlOKAlOKAlOKAlOKAlOKAlA==";

        public static Color windowBackground => EditorGUIUtility.isProSkin ? DarkBackgroundColor : LightBackgroundColor;
        public static Color windowAccent => EditorGUIUtility.isProSkin ? DarkBorder : LightBorder;

        public static Color outlineColor =>
            EditorGUIUtility.isProSkin ? new Color32(80, 80, 80, 255) : new Color32(176, 176, 176, 255);

        public static Texture2D gridTexture => _gridTexture == null
            ? _gridTexture = GenerateGridTexture(Color.Lerp(Color.white, windowAccent, 0.8f), windowAccent, false)
            : _gridTexture;

        public static Texture2D gridTexture2x => _gridTexture2x == null
            ? _gridTexture2x = GenerateGridTexture(Color.Lerp(Color.white, windowAccent, 0.8f), windowAccent, true)
            : _gridTexture2x;

        public static Texture2D favoriteDisabled => _favoriteDisabled == null
            ? _favoriteDisabled = FindTexture("QuickSearch/favorite_disabled")
            : _favoriteDisabled;

        public static Texture2D favoriteEnabled => _favoriteEnabled == null
            ? _favoriteEnabled = FindTexture("QuickSearch/favorite_enabled")
            : _favoriteEnabled;

        public static Texture2D folder => _folder == null
            ? _folder = (Texture2D)EditorGUIUtility.IconContent("Folder Icon").image
            : _folder;

        public static Texture2D folderOpen =>
            _folderOpen == null ? _folderOpen = FindTexture("FolderOpened Icon") : _folderOpen;

        public static Texture2D next => _next == null ? _next = FindTexture("tab_next") : _next;
        public static Texture2D prev => _prev == null ? _prev = FindTexture("tab_prev") : _prev;
        public static Texture2D menu => _menu == null ? _menu = FindTexture("_Menu") : _menu;

        public static Texture2D inspectorIcon => _inspectorIcon == null
            ? _inspectorIcon = FindTexture("UnityEditor.InspectorWindow")
            : _inspectorIcon;

        public static Texture2D hiearchyIcon => _hiearchyIcon == null
            ? _hiearchyIcon = FindTexture("UnityEditor.HierarchyWindow")
            : _hiearchyIcon;

        public static Texture2D searchBackground => _searchBackground == null
            ? _searchBackground = Resources.Load<Texture2D>("search_bg")
            : _searchBackground;

        public static Texture2D solid => _solid == null ? _solid = Resources.Load<Texture2D>("Misc/px") : _solid;
        public static Texture2D curve => _curve == null ? _curve = Resources.Load<Texture2D>("curve") : _curve;

        public static Texture2D inConnectionOutline => _inConnectionOutline == null
            ? _inConnectionOutline = Resources.Load<Texture2D>("in_connection_outline")
            : _inConnectionOutline;

        public static Texture2D foldout =>
            _foldout == null ? _foldout = Resources.Load<Texture2D>("foldout") : _foldout;

        public static Texture2D moveUp => _moveUp == null ? _moveUp = Resources.Load<Texture2D>("move_up") : _moveUp;

        public static Texture2D moveDown =>
            _moveDown == null ? _moveDown = Resources.Load<Texture2D>("move_down") : _moveDown;

        public static Texture2D close => _close == null ? _close = Resources.Load<Texture2D>("close") : _close;

        public static GUIStyle title
        {
            get
            {
                if (_title == null)
                {
                    _title = new GUIStyle();
                    _title.alignment = TextAnchor.UpperCenter;
                    _title.fontSize = 24;
                    _title.normal.textColor = Color.white;
                }

                return _title;
            }
        }

        public static GUIStyle quickSearch
        {
            get
            {
                if (_quickSearch == null)
                {
                    _quickSearch = new GUIStyle();
                    _quickSearch.normal.background = Icons.GetResource("QuickSearch/search_bg");
                    _quickSearch.border = new RectOffset(8, 8, 8, 8);
                    _quickSearch.padding = new RectOffset(2, 2, 2, 2);
                }

                return _quickSearch;
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
                    _favoriteToggle.normal.background = favoriteDisabled;
                    _favoriteToggle.onNormal.background = favoriteEnabled;
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
                    _searchResult.hover.textColor = Color.white;
                    _searchResult.normal.textColor = Color.white;
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
                    _nodeLabel.imagePosition = ImagePosition.ImageAbove;
                    _nodeLabel.fontSize = 15;
                    _nodeLabel.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
                }

                return _nodeLabel;
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
                    _blackboardEntry.normal.textColor = Color.white;
                    _blackboardEntry.hover.textColor = Color.white;
                    _blackboardEntry.hover.background =
                        GenerateSolid(new Color(0.5f, 0.5f, 0.5f, 0.5f), Vector2Int.one);
                    _blackboardEntry.active.textColor = Color.white;
                    _blackboardEntry.active.background =
                        GenerateSolid(GUI.skin.settings.selectionColor, Vector2Int.one);
                    _blackboardEntry.focused.textColor = Color.white;
                    _blackboardEntry.focused.background = _blackboardEntry.active.background;
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

        public static string megamind
        {
            get
            {
                byte[] data = Convert.FromBase64String(_megamind);
                string decodedString = Encoding.UTF8.GetString(data);

                return decodedString;
            }
        }

        private static Texture2D FindTexture(string path)
        {
            bool darkMode = EditorGUIUtility.isProSkin;

            string name = (darkMode ? "d_" : "") + Path.GetFileName(path);

            Texture2D tex = Resources.Load<Texture2D>(Path.Join(Path.GetDirectoryName(path), name));

            if (tex != null)
                return tex;

            tex = (Texture2D)EditorGUIUtility.IconContent(name).image;

            if (tex != null)
                return tex;

            return EditorGUIUtility.FindTexture(name);
        }

        private static Texture2D GenerateGridTexture(Color dots, Color bg, bool large)
        {
            Texture2D tex = new Texture2D(64, 64);
            Color[] cols = new Color[64 * 64];
            for (int y = 0; y < 64; y++)
            for (int x = 0; x < 64; x++)
            {
                Color col = bg;

                if (!large && (y % 16 == 0 || x % 16 == 0))
                    col = Color.Lerp(dots, bg, 0.65f);

                if (y == 0 || x == 0) col = Color.Lerp(dots, bg, 0.65f);
                if (y == 63 || x == 63) col = Color.Lerp(dots, bg, 0.35f);

                cols[y * 64 + x] = col;
            }

            tex.SetPixels(cols);
            tex.wrapMode = TextureWrapMode.Repeat;
            tex.filterMode = FilterMode.Bilinear;
            tex.name = "Grid";
            tex.Apply();
            return tex;
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

        private static Texture2D GenerateCrossTexture(Color line)
        {
            Texture2D tex = new Texture2D(64, 64);
            Color[] cols = new Color[64 * 64];
            for (int y = 0; y < 64; y++)
            for (int x = 0; x < 64; x++)
            {
                Color col = line;
                if (y != 31 && x != 31) col.a = 0;
                cols[y * 64 + x] = col;
            }

            tex.SetPixels(cols);
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.filterMode = FilterMode.Bilinear;
            tex.name = "Grid";
            tex.Apply();
            return tex;
        }
    }
}