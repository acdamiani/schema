using UnityEditor;
using UnityEngine;
using Schema.Utilities;

internal static class Styles
{
    //Colors
    public static Color windowBackground => EditorGUIUtility.isProSkin ? DarkBackgroundColor : LightBackgroundColor;
    public static Color windowAccent => EditorGUIUtility.isProSkin ? DarkBorder : LightBorder;
    public static Color outlineColor => EditorGUIUtility.isProSkin ? new Color32(80, 80, 80, 255) : new Color32(176, 176, 176, 255);
    public static readonly Color lowerPriorityColor = new Color32(255, 140, 144, 255);
    public static readonly Color selfColor = new Color32(71, 255, 166, 255);
    private static readonly Color DarkBackgroundColor = new Color32(56, 56, 56, 255);
    private static readonly Color LightBackgroundColor = new Color32(200, 200, 200, 255);
    private static readonly Color DarkBorder = new Color32(40, 40, 40, 255);
    private static readonly Color LightBorder = new Color32(147, 147, 147, 255);
    private static Texture2D _nodeSelected;
    private static Texture2D _arrow;
    private static Texture2D _nodeSelectedDecorator;
    private static Texture2D _blackboardIcon;
    private static Texture2D _dropdown;
    private static Texture2D _warnIcon;
    private static Texture2D _errorIcon;
    private static Texture2D _infoIcon;
    private static Texture2D _searchIcon;
    private static Texture2D _splashImage;
    private static Texture2D _plus;
    private static Texture2D _minus;
    private static Texture2D _gridTexture;
    private static Texture2D _gridTexture2x;
    private static Texture2D _circle;
    private static Texture2D _global;
    private static Texture2D _local;
    private static Texture2D _shared;
    private static Texture2D _preAudioLoopOff;
    private static GUIContent _visibilityToggleOffContent;
    private static GUIContent _visibilityToggleOnContent;
    public static Texture2D warnIcon => _warnIcon != null ? _warnIcon : _warnIcon = EditorGUIUtility.FindTexture("console.warnicon");
    public static Texture2D errorIcon => _errorIcon != null ? _errorIcon : _errorIcon = EditorGUIUtility.FindTexture("console.erroricon");
    public static Texture2D infoIcon => _infoIcon != null ? _infoIcon : _infoIcon = EditorGUIUtility.FindTexture("console.infoicon");
    public static Texture2D searchIcon => _searchIcon != null ? _searchIcon : _searchIcon = EditorGUIUtility.FindTexture("Search Icon");
    public static Texture2D splashImage => _splashImage != null ? _splashImage : _splashImage = Resources.Load<Texture2D>("splash");
    public static Texture2D nodeSelected => _nodeSelected != null ? _nodeSelected : _nodeSelected = Resources.Load<Texture2D>("node_highlight");
    public static Texture2D arrow => _arrow != null ? _arrow : _arrow = Resources.Load<Texture2D>("arrow");
    public static Texture2D blackboardIcon => _blackboardIcon != null ? _blackboardIcon : _blackboardIcon = Resources.Load<Texture2D>("blackboard_key");
    public static Texture2D plus => _plus != null ? _plus : _plus = EditorGUIUtility.FindTexture("Toolbar Plus More");
    public static Texture2D minus => _minus != null ? _minus : _minus = EditorGUIUtility.FindTexture("Toolbar Minus");
    public static Texture2D circle => _circle != null ? _circle : _circle = Resources.Load<Texture2D>("in_connection");
    public static Texture2D local => _local != null ? _local : _local = (Texture2D)EditorGUIUtility.IconContent("ModelImporter Icon").image;
    public static Texture2D global => _global != null ? _global : _global = EditorGUIUtility.FindTexture("Profiler.GlobalIllumination");
    public static Texture2D shared => _shared != null ? _shared : _shared = EditorGUIUtility.FindTexture("Linked");
    public static Texture2D preAudioLoopOff => _preAudioLoopOff != null ? _preAudioLoopOff : _preAudioLoopOff = EditorGUIUtility.FindTexture("preAudioLoopOff@2x");
    public static GUIContent visibilityToggleOffContent => _visibilityToggleOffContent != null ? _visibilityToggleOffContent : _visibilityToggleOffContent = new GUIContent(EditorGUIUtility.FindTexture("animationvisibilitytoggleoff"), "Toggle Inspector On");
    public static GUIContent visibilityToggleOnContent => _visibilityToggleOnContent != null ? _visibilityToggleOnContent : _visibilityToggleOnContent = new GUIContent("", EditorGUIUtility.FindTexture("animationvisibilitytoggleon"), "Toggle Inspector Off");
    public static Texture2D gridTexture => _gridTexture == null ? _gridTexture = GenerateGridTexture(Color.Lerp(Color.grey, windowAccent, 0.5f), windowAccent, false) : _gridTexture;
    public static Texture2D gridTexture2x => _gridTexture2x == null ? _gridTexture2x = GenerateGridTexture(Color.Lerp(Color.grey, windowAccent, 0.5f), windowAccent, true) : _gridTexture2x;
    private static Texture2D _favoriteDisabled;
    public static Texture2D favoriteDisabled => _favoriteDisabled == null ? _favoriteDisabled = FindTexture("QuickSearch/favorite_disabled") : _favoriteDisabled;
    private static Texture2D _favoriteEnabled;
    public static Texture2D favoriteEnabled => _favoriteEnabled == null ? _favoriteEnabled = FindTexture("QuickSearch/favorite_enabled") : _favoriteEnabled;
    private static Texture2D _folder;
    public static Texture2D folder => _folder == null ? _folder = (Texture2D)EditorGUIUtility.IconContent("Folder Icon").image : _folder;
    private static Texture2D _folderOpen;
    public static Texture2D folderOpen => _folderOpen == null ? _folderOpen = FindTexture("FolderOpened Icon") : _folderOpen;
    private static Texture2D _next;
    public static Texture2D next => _next == null ? _next = FindTexture("tab_next") : _next;
    private static Texture2D _prev;
    public static Texture2D prev => _prev == null ? _prev = FindTexture("tab_prev") : _prev;
    private static Texture2D _menu;
    public static Texture2D menu => _menu == null ? _menu = FindTexture("_Menu") : _menu;
    private static Texture2D _inspectorIcon;
    public static Texture2D inspectorIcon => _inspectorIcon == null ? _inspectorIcon = FindTexture("UnityEditor.InspectorWindow") : _inspectorIcon;
    private static Texture2D _hiearchyIcon;
    public static Texture2D hiearchyIcon => _hiearchyIcon == null ? _hiearchyIcon = FindTexture("UnityEditor.HierarchyWindow") : _hiearchyIcon;
    private static Texture2D _searchBackground;
    public static Texture2D searchBackground => _searchBackground == null ? _searchBackground = Resources.Load<Texture2D>("search_bg") : _searchBackground;
    private static Texture2D _solid;
    public static Texture2D solid => _solid == null ? _solid = Resources.Load<Texture2D>("Misc/px") : _solid;
    private static Texture2D _curve;
    public static Texture2D curve => _curve == null ? _curve = Resources.Load<Texture2D>("curve") : _curve;
    private static Texture2D _inConnectionOutline;
    public static Texture2D inConnectionOutline => _inConnectionOutline == null ? _inConnectionOutline = Resources.Load<Texture2D>("in_connection_outline") : _inConnectionOutline;
    private static GUIStyle _quickSearch;
    public static GUIStyle quickSearch
    {
        get
        {
            if (_quickSearch == null)
            {
                _quickSearch = new GUIStyle();
                _quickSearch.normal.background = searchBackground;
                _quickSearch.border = new RectOffset(2, 2, 2, 2);
                _quickSearch.padding = new RectOffset(2, 2, 2, 2);
            }

            return _quickSearch;
        }
    }
    private static GUIStyle _blackboardScroll;
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
    private static GUIStyle _favoriteToggle;
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
    private static GUIStyle _padding8x;
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
    private static GUIStyle _searchResult;
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
                _searchResult.hover.background = roundedBox.normal.background.Tint(GUI.skin.settings.selectionColor);
                _searchResult.hover.textColor = Color.white;
                _searchResult.normal.textColor = Color.white;
            }

            return _searchResult;
        }
    }
    private static GUIStyle _shadow;
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
    private static GUIStyle _roundedBox;
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
                _roundedBox.normal.textColor = Color.white;
            }

            return _roundedBox;
        }
    }
    private static GUIStyle _nodeLabel;
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
                _nodeLabel.normal.textColor = Color.white;
            }

            return _nodeLabel;
        }
    }
    private static GUIStyle _conditional;
    public static GUIStyle conditional
    {
        get
        {
            if (_conditional == null)
            {
                _conditional = new GUIStyle();
                _conditional.alignment = TextAnchor.MiddleRight;
                _conditional.imagePosition = ImagePosition.TextOnly;
                _conditional.fontSize = 15;
                _conditional.normal.textColor = Color.white;
                _conditional.normal.background = Resources.Load<Texture2D>("round");
                _conditional.border = new RectOffset(8, 8, 8, 8);
                _conditional.padding = new RectOffset(8, 8, 8, 8);
            }

            return _conditional;
        }
    }
    private static StylesObj _styles;
    public static StylesObj styles => _styles ??= new StylesObj();
    private static string _megamind =
        "4oCU4oCU4oCU4oCU4oCU4oCU4oCU4oCU4oCUTm8gZW50cmllcz/igJTigJTigJTigJTigJTigJTigJTigJTigJQK4qCA4qOe4qK94qKq4qKj4qKj4qKj4qKr4qG64qG14qOd4qGu4qOX4qK34qK94qK94qK94qOu4qG34qG94qOc4qOc4qKu4qK64qOc4qK34qK94qKd4qG94qOdCuKguOKhuOKgnOKgleKgleKggeKigeKih+Kij+KiveKiuuKjquKhs+KhneKjjuKjj+Kir+KinuKhv+Kjn+Kjt+Kjs+Kir+Kht+KjveKiveKir+Kjs+Kjq+KghwrioIDioIDiooDiooDiooTioqzioqrioarioY7io4bioYjioJrioJzioJXioIfioJfioJ3iopXioq/ioqvio57io6/io7/io7viob3io4/iopfio5fioI/ioIAK4qCA4qCq4qGq4qGq4qOq4qKq4qK64qK44qKi4qKT4qKG4qKk4qKA4qCA4qCA4qCA4qCA4qCI4qKK4qKe4qG+4qO/4qGv4qOP4qKu4qC34qCB4qCA4qCACuKggOKggOKggOKgiOKgiuKghuKhg+KgleKileKih+Kih+Kih+Kih+Kih+Kij+KijuKijuKihuKihOKggOKikeKjveKjv+KineKgsuKgieKggOKggOKggOKggArioIDioIDioIDioIDioIDiob/ioILioKDioIDioYfioofioJXioojio4DioIDioIHioKHioKPioaPioavio4Lio7/ioK/ioqrioLDioILioIDioIDioIDioIAK4qCA4qCA4qCA4qCA4qGm4qGZ4qGC4qKA4qKk4qKj4qCj4qGI4qO+4qGD4qCg4qCE4qCA4qGE4qKx4qOM4qO24qKP4qKK4qCC4qCA4qCA4qCA4qCA4qCA4qCACuKggOKggOKggOKggOKineKhsuKjnOKhruKhj+KijuKijOKiguKgmeKgouKgkOKigOKimOKiteKjveKjv+Khv+KggeKggeKggOKggOKggOKggOKggOKggOKggArioIDioIDioIDioIDioKjio7riobrioZXioZXiobHioZHioYbioZXioYXioZXioZziobzior3iobvioI/ioIDioIDioIDioIDioIDioIDioIDioIDioIDioIAK4qCA4qCA4qCA4qCA4qO84qOz4qOr4qO+4qO14qOX4qG14qGx4qGh4qKj4qKR4qKV4qKc4qKV4qGd4qCA4qCA4qCA4qCA4qCA4qCA4qCA4qCA4qCA4qCA4qCACuKggOKggOKggOKjtOKjv+KjvuKjv+Kjv+Kjv+Khv+KhveKhkeKijOKgquKhouKho+Kjo+Khn+KggOKggOKggOKggOKggOKggOKggOKggOKggOKggOKggOKggArioIDioIDioIDioZ/iob7io7/ior/ior/iorXio73io77io7zio5jiorjiorjio57ioZ/ioIDioIDioIDioIDioIDioIDioIDioIDioIDioIDioIDioIDioIAK4qCA4qCA4qCA4qCA4qCB4qCH4qCh4qCp4qGr4qK/4qOd4qG74qGu4qOS4qK94qCL4qCA4qCA4qCA4qCA4qCA4qCA4qCA4qCA4qCA4qCA4qCA4qCA4qCA4qCACuKAlOKAlOKAlOKAlOKAlOKAlOKAlOKAlOKAlOKAlOKAlOKAlOKAlOKAlOKAlOKAlOKAlOKAlOKAlOKAlOKAlOKAlOKAlOKAlOKAlA==";
    public static string megamind
    {
        get
        {
            byte[] data = System.Convert.FromBase64String(_megamind);
            string decodedString = System.Text.Encoding.UTF8.GetString(data);

            return decodedString;
        }
    }
    private static Texture2D FindTexture(string path)
    {
        bool darkMode = EditorGUIUtility.isProSkin;

        string name = (darkMode ? "d_" : "") + System.IO.Path.GetFileName(path);

        Texture2D tex = Resources.Load<Texture2D>(System.IO.Path.Join(System.IO.Path.GetDirectoryName(path), name));

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
        {
            for (int x = 0; x < 64; x++)
            {
                Color col = bg;

                if (!large && (y % 16 == 0 || x % 16 == 0))
                    col = Color.Lerp(dots, bg, 0.65f);

                if (y == 0 || x == 0) col = Color.Lerp(dots, bg, 0.65f);
                if (y == 63 || x == 63) col = Color.Lerp(dots, bg, 0.35f);

                cols[(y * 64) + x] = col;
            }
        }
        tex.SetPixels(cols);
        tex.wrapMode = TextureWrapMode.Repeat;
        tex.filterMode = FilterMode.Bilinear;
        tex.name = "Grid";
        tex.Apply();
        return tex;
    }
    public static Texture2D GenerateSolid(Color color, Vector2Int size)
    {
        Texture2D tex = new Texture2D(size.y, size.x);
        for (int y = 0; y < size.y; y++)
        {
            for (int x = 0; x < size.x; x++)
            {
                tex.SetPixel(x, y, color);
            }
        }
        tex.wrapMode = TextureWrapMode.Repeat;
        tex.name = "Solid"; tex.Apply(); return tex;
    }
    private static Texture2D GenerateCrossTexture(Color line)
    {
        Texture2D tex = new Texture2D(64, 64);
        Color[] cols = new Color[64 * 64];
        for (int y = 0; y < 64; y++)
        {
            for (int x = 0; x < 64; x++)
            {
                Color col = line;
                if (y != 31 && x != 31) col.a = 0;
                cols[(y * 64) + x] = col;
            }
        }
        tex.SetPixels(cols);
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;
        tex.name = "Grid";
        tex.Apply();
        return tex;
    }
    public class StylesObj
    {
        public readonly GUIStyle node, nodeWithoutPadding, decorator, title, nodeLabel, nodeText, nodeSelected, newNode, addNodeWindow, backgroundBg, searchbar, searchResult, minimap, nameField;
        ///<summary>
        ///Generates Styles object which is used in the Node Editor GUI
        ///</summary>
        public StylesObj()
        {
            //selected node style
            nodeSelected = new GUIStyle
            {
                normal =
                {
                    background = global::Styles.nodeSelected
                },
                border = new RectOffset(8, 8, 8, 8)
            };

            decorator = new GUIStyle
            {
                normal =
                {
                    background = global::Styles.roundedBox.normal.background
                },
                border = new RectOffset(8, 8, 8, 8),
                contentOffset = Vector2.zero,
                margin = new RectOffset(0, 0, 0, 0),
                padding = new RectOffset(0, 0, 0, 0)
            };

            title = new GUIStyle("LargeLabel")
            {
                alignment = TextAnchor.MiddleCenter
            };

            nodeLabel = new GUIStyle("Label")
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 15
            };

            nodeText = new GUIStyle
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 12,
                normal =
                {
                    textColor = Color.white
                }
            };

            addNodeWindow = new GUIStyle(GUI.skin.window)
            {
                normal =
                {
                    background = GenerateSolid(windowAccent, Vector2Int.one * 32)
                },
                onNormal =
                {
                    background = GenerateSolid(windowAccent, Vector2Int.one * 32)
                },
                margin = new RectOffset(0, 0, 0, 0),
                padding = new RectOffset(0, 0, 16, 0)
            };

            backgroundBg = new GUIStyle
            {
                normal =
                {
                    background = GenerateSolid(windowBackground, Vector2Int.one * 32)
                }
            };

            searchbar = new GUIStyle
            {
                alignment = TextAnchor.MiddleLeft,
                normal =
                {
                    textColor = Color.white
                }
            };

            searchResult = new GUIStyle
            {
                alignment = TextAnchor.MiddleLeft,
                normal =
                {
                    textColor = Color.white,
                    background = GenerateSolid(Color.white, Vector2Int.one * 32)
                },
                margin = new RectOffset(0, 0, 0, 0),
                padding = new RectOffset(8, 8, 8, 8),
                hover =
                {
                    textColor = Color.gray
                },
            };

            nameField = new GUIStyle("PR TextField");
        }
    }
}
