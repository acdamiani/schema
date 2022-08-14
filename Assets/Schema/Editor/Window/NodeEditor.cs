using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Schema;
using Schema.Internal;
using Schema.Utilities;
using SchemaEditor.Internal;
using SchemaEditor.Internal.ComponentSystem.Components;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.ShortcutManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SchemaEditor
{
    //This class contains the basic information about the EditorWindow, including various preferences and basic methods (Delete, Select, etc.)
    public partial class NodeEditor : EditorWindow, IHasCustomMenu, ICanvasContextProvider
    {
        public static NodeEditor instance;
        private static Dictionary<Type, List<Type>> nodeTypes;
        private static Type[] decoratorTypes;
        private static readonly List<Object> copyBuffer = new();
        public Node requestingConnection;
        public Graph target;
        public Blackboard globalBlackboard;
        public Window windowInfo = new();
        private int? _controlID;
        public Event eventNoZoom;
        private int nodeCount;

        private Node orphanNode;

        //Validates connections between nodes also resets HideFlags
        private void OnEnable()
        {
            if (windowInfo.inspectorToggled)
                window = new Rect(0f, 0f, position.width - windowInfo.inspectorWidth - Window.padding * 2,
                    position.height);
            else
                window = new Rect(0f, 0f, position.width, position.height);

            if (target != null)
                RebuildComponentTree();

            wantsMouseMove = true;

            Undo.undoRedoPerformed += UndoPerformed;

            instance = this;
            if (target != null && target.blackboard != null)
                Blackboard.instance = target.blackboard;

            target?.PurgeNull();
        }

        private void OnDestroy()
        {
            Undo.undoRedoPerformed -= UndoPerformed;
            Undo.ClearAll();

            foreach (SchemaAgent agent in FindObjectsOfType<SchemaAgent>())
                // agent.editorTarget = null;
                SceneView.RepaintAll();

            instance = null;
            Blackboard.instance = null;
            DestroyImmediate(editor);
            DestroyImmediate(blackboardEditor);
        }

        public int GetControlID()
        {
            if (_controlID == null)
                _controlID = GUIUtility.GetControlID(FocusType.Passive);

            return _controlID.Value;
        }

        public Rect GetRect()
        {
            return new Rect(0f, tabHeight, position.width, position.height);
        }

        public Rect GetViewRect()
        {
            return window;
        }

        public EditorWindow GetEditorWindow()
        {
            return this;
        }

        public void Rebuild()
        {
            RebuildComponentTree();
        }

        public float GetToolbarHeight()
        {
            try
            {
                return EditorStyles.toolbar.fixedHeight;
            }
            catch
            {
                return 0f;
            }
        }

        public ComponentCanvas GetCanvas()
        {
            return canvas;
        }

        void IHasCustomMenu.AddItemsToMenu(GenericMenu menu)
        {
            menu.AddItem("Preferences", windowInfo.settingsShown, TogglePrefs, false);
            menu.AddItem("Documentation", false,
                () => OpenUrl("https://thinking-automation.vercel.app/docs/getting-started"), false);
        }

        [DidReloadScripts]
        private static void Init()
        {
            nodeTypes = new Dictionary<Type, List<Type>>();
            foreach (Type t in HelperMethods.GetNodeTypes())
            {
                IEnumerable<Type> test = HelperMethods.GetEnumerableOfType(t);
                //Debug.Log(t + ": " + String.Join(",", test));
                nodeTypes.Add(t, test.ToList());
            }

            decoratorTypes = HelperMethods.GetEnumerableOfType(typeof(Conditional)).ToArray();
        }

        [MenuItem("Window/AI/Behavior Editor")]
        private static void OpenWindow()
        {
            NodeEditor window = GetWindow<NodeEditor>();
            window.Open(null);
        }

        public static void OpenGraph(Graph graphObj)
        {
            NodeEditor window = GetWindow<NodeEditor>();
            window.Open(graphObj);
        }

        public void Open(Graph graphObj)
        {
            target = graphObj;
            target?.Initialize();

            canvas = null;
            RebuildComponentTree();

            windowInfo = new Window();
            windowInfo.editor = this;
            instance = this;

            if (target == null)
            {
                titleContent = new GUIContent("Behavior Editor");
                return;
            }

            titleContent = new GUIContent(target.name);

            string id = AssetDatabase.GUIDFromAssetPath(AssetDatabase.GetAssetPath(target)).ToString();
            string last = EditorPrefs.GetString("Schema Recently Opened");
            List<string> lastFiles = last.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();

            if (lastFiles.Contains(id))
                lastFiles.MoveItemAtIndexToFront(lastFiles.IndexOf(id));
            else
                lastFiles.Insert(0, id);

            List<string> f = new List<string>(lastFiles);

            foreach (string s in f)
            {
                string path = AssetDatabase.GUIDToAssetPath(s);

                if (string.IsNullOrEmpty(path))
                {
                    lastFiles.Remove(s);
                    continue;
                }

                string absolutePath = Application.dataPath + path.Substring(6);

                if (!File.Exists(absolutePath))
                    lastFiles.Remove(s);
            }

            EditorPrefs.SetString("Schema Recently Opened", string.Join(",", lastFiles));

            Undo.ClearAll();

            Blackboard.instance = target.blackboard;
        }

        private void TogglePrefs()
        {
            windowInfo.settingsShown = !windowInfo.settingsShown;
            windowInfo.inspectorScroll = Vector2.zero;
            windowInfo.inspectorToggled = !windowInfo.inspectorToggled ? true : windowInfo.inspectorToggled;
        }

        private static GUIContent AggregateErrors(List<Error> errors)
        {
            GUIContent ret = new GUIContent();

            if (errors.Count == 0) return GUIContent.none;

            Error.Severity severity = (Error.Severity)errors.Max(error => (int)error.severity);
            switch (severity)
            {
                case Error.Severity.Info:
                    ret.image = Icons.GetEditor("console.infoicon");
                    break;
                case Error.Severity.Warning:
                    ret.image = Icons.GetEditor("console.warnicon");
                    break;
                case Error.Severity.Error:
                    ret.image = Icons.GetEditor("console.erroricon");
                    break;
            }

            string total = "";
            for (int i = 0; i < errors.Count; i++)
            {
                string s = errors[i].message;

                total += (i == 0 ? "" : "\n") + s;
            }

            ret.tooltip = total;

            return ret;
        }

        private static GUIContent GetErrors(Node node)
        {
            List<Error> errors = new List<Error>(node.GetErrors());

            // foreach (Decorator d in node.decorators)
            //     errors.AddRange(d.GetErrors().Select(error => new Error($"{error.message} ({d.name})", error.severity)));

            if (node.priority < 1) errors.Add(new Error("Node not connected to root!", Error.Severity.Warning));
            else if (node.children.Length == 0 && node.CanHaveChildren())
                errors.Add(new Error("No child node attatched", Error.Severity.Warning));

            return AggregateErrors(errors);
        }

        private void UndoPerformed()
        {
            RebuildComponentTree();
            target.Traverse();
        }

        private void OpenUrl(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
        }


        [OnOpenAsset(1)]
        public static bool Open(int instanceID, int line)
        {
            Object obj = EditorUtility.InstanceIDToObject(instanceID);
            if (obj as Graph != null)
            {
                OpenGraph((Graph)obj);
                return true;
            }

            return false;
        }

        //--SCHEMA SHORTCUTS--//
        [Shortcut("Schema/Add Node", KeyCode.A, ShortcutModifiers.Shift)]
        public static void AddNodeCommand()
        {
            if (instance == null) return;

            if (!Application.isPlaying)
                instance.CreateAddNodeWindow();
        }

        [Shortcut("Schema/Add Conditional", KeyCode.B, ShortcutModifiers.Shift)]
        public static void AddConditionalCommand()
        {
            if (instance == null && instance.CanAddConditional()) return;

            if (!instance.editingPaused)
                instance.CreateAddConditionalWindow();
        }

        [Shortcut("Schema/Break Connections", KeyCode.B, ShortcutModifiers.Action | ShortcutModifiers.Alt)]
        public static void BreakConnectionsCommand()
        {
            if (instance == null) return;

            instance.target.BreakConnections(
                instance.canvas.selected
                    .Where(x => x is NodeComponent)
                    .Cast<NodeComponent>()
                    .Select(x => x.node)
            );
        }

        internal static class Prefs
        {
            public static bool saveOnClose
            {
                get => EditorPrefs.GetBool("SCHEMA_PREF__saveOnClose", false);
                set => EditorPrefs.SetBool("SCHEMA_PREF__saveOnClose", value);
            }

            public static bool formatOnSave
            {
                get => EditorPrefs.GetBool("SCHEMA_PREF__formatOnSave", true);
                set => EditorPrefs.SetBool("SCHEMA_PREF__formatOnSave", value);
            }

            public static string screenshotPath
            {
                get => EditorPrefs.GetString("SCHEMA_PREF__screenshotPath", "Screenshots");
                set => EditorPrefs.SetString("SCHEMA_PREF__screenshotPath", value);
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

            public static bool liveLink
            {
                get => EditorPrefs.GetBool("SCHEMA_PREF__liveLink", false);
                set => EditorPrefs.SetBool("SCHEMA_PREF__liveLink", value);
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
}