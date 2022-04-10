using UnityEditor;
using UnityEngine;

public static class NodeEditorResources
{
    //Colors
    public static Color windowBackground => EditorGUIUtility.isProSkin ? DarkBackgroundColor : LightBackgroundColor;
    public static Color windowAccent => EditorGUIUtility.isProSkin ? DarkBorder : LightBorder;
    public static readonly Color32 lowerPriorityColor = new Color32(255, 140, 144, 255);
    public static readonly Color32 selfColor = new Color32(71, 255, 166, 255);
    private static readonly Color32 DarkBackgroundColor = new Color32(56, 56, 56, 255);
    private static readonly Color32 LightBackgroundColor = new Color32(200, 200, 200, 255);
    private static readonly Color32 DarkBorder = new Color32(40, 40, 40, 255);
    private static readonly Color32 LightBorder = new Color32(147, 147, 147, 255);
    //Textures
    private static Texture2D _node;
    private static Texture2D _nodeSelected;
    private static Texture2D _arrow;
    private static Texture2D _nodeSelectedDecorator;
    private static Texture2D _solid;
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
    private static Texture2D _circle;
    private static Texture2D _global;
    private static Texture2D _local;
    private static Texture2D _shared;
    private static Texture2D _preAudioLoopOff;
    public static Texture2D warnIcon => _warnIcon != null ? _warnIcon : _warnIcon = EditorGUIUtility.FindTexture("console.warnicon");
    public static Texture2D errorIcon => _errorIcon != null ? _errorIcon : _errorIcon = EditorGUIUtility.FindTexture("console.erroricon");
    public static Texture2D infoIcon => _infoIcon != null ? _infoIcon : _infoIcon = EditorGUIUtility.FindTexture("console.infoicon");
    public static Texture2D searchIcon => _searchIcon != null ? _searchIcon : _searchIcon = EditorGUIUtility.FindTexture("Search Icon");
    public static Texture2D splashImage => _splashImage != null ? _splashImage : _splashImage = Resources.Load<Texture2D>("splash");
    private static Texture2D node => _node != null ? _node : _node = Resources.Load<Texture2D>("node");
    private static Texture2D nodeSelected => _nodeSelected != null ? _nodeSelected : _nodeSelected = Resources.Load<Texture2D>("node_highlight");
    public static Texture2D arrow => _arrow != null ? _arrow : _arrow = Resources.Load<Texture2D>("arrow");
    public static Texture2D blackboardIcon => _blackboardIcon != null ? _blackboardIcon : _blackboardIcon = Resources.Load<Texture2D>("blackboard_key");
    public static Texture2D plus => _plus != null ? _plus : _plus = EditorGUIUtility.FindTexture("Toolbar Plus More");
    public static Texture2D minus => _minus != null ? _minus : _minus = EditorGUIUtility.FindTexture("Toolbar Minus");
    public static Texture2D circle => _circle != null ? _circle : _circle = Resources.Load<Texture2D>("Circle");
    public static Texture2D solid => _solid != null ? _solid : _solid = GenerateSolid(Color.white, new Vector2Int(32, 32));
    public static Texture2D local => _local != null ? _local : _local = (Texture2D)EditorGUIUtility.IconContent("ModelImporter Icon").image;
    public static Texture2D global => _global != null ? _global : _global = EditorGUIUtility.FindTexture("Profiler.GlobalIllumination");
    public static Texture2D shared => _shared != null ? _shared : _shared = EditorGUIUtility.FindTexture("Linked");
    public static Texture2D preAudioLoopOff => _preAudioLoopOff != null ? _preAudioLoopOff : _preAudioLoopOff = EditorGUIUtility.FindTexture("preAudioLoopOff@2x");
    public static Texture2D gridTexture
    {
        get
        {
            if (_gridTexture == null) _gridTexture = GenerateGridTexture(Color.grey, windowBackground);
            return _gridTexture;
        }
    }
    private static Texture2D _crossTexture;
    public static Texture2D crossTexture
    {
        get
        {
            if (_crossTexture == null) _crossTexture = GenerateCrossTexture(Color.grey);
            return _crossTexture;
        }
    }
    private static Styles _styles;
    public static Styles styles => _styles ??= new Styles();

    private static Texture2D GenerateGridTexture(Color line, Color bg)
    {
        Texture2D tex = new Texture2D(64, 64);
        Color[] cols = new Color[64 * 64];
        for (int y = 0; y < 64; y++)
        {
            for (int x = 0; x < 64; x++)
            {
                Color col = bg;
                if (y % 16 == 0 || x % 16 == 0) col = Color.Lerp(line, bg, 0.65f);
                if (y == 63 || x == 63) col = Color.Lerp(line, bg, 0.35f);
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
    public class Styles
    {
        public readonly GUIStyle node, decorator, title, nodeLabel, nodeText, nodeSelected, newNode, addNodeWindow, backgroundBg, searchbar, searchResult, minimap, nameField;
        ///<summary>
        ///Generates Styles object which is used in the Node Editor GUI
        ///</summary>
        public Styles()
        {
            //node style
            node = new GUIStyle
            {
                normal =
                {
                    background = NodeEditorResources.node
                },
                border = new RectOffset(8, 8, 8, 8),
                padding = new RectOffset(16, 16, 16, 16)
            };

            //selected node style
            nodeSelected = new GUIStyle
            {
                normal =
                {
                    background = NodeEditorResources.nodeSelected
                },
                border = new RectOffset(8, 8, 8, 8)
            };

            decorator = new GUIStyle
            {
                normal =
                {
                    background = NodeEditorResources.node
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
                fontSize = 10,
                normal =
                {
                    textColor = Color.white
                }
            };

            //new node result item style
            newNode = new GUIStyle
            {
                normal =
                {
                    background = NodeEditorResources.node,
                    textColor = Color.black
                },
                border = new RectOffset(32, 32, 32, 32),
                padding = new RectOffset(0, 0, 4, 16),
                alignment = TextAnchor.MiddleCenter
            };

            addNodeWindow = new GUIStyle
            {
                normal =
                {
                    background = GenerateSolid(windowAccent, Vector2Int.one * 32)
                },
                onNormal =
                {
                    background = GenerateSolid(windowAccent, Vector2Int.one * 32)
                }
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
                }
            };

            nameField = new GUIStyle("PR TextField");
            //TODO: Make this not magic
            nameField.fixedWidth = 250f;
        }
    }
}
