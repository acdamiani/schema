using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Schema;
using Schema.Internal;
using Schema.Utilities;
using SchemaEditor.Utilities;
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
            minSize = new Vector2(Window.inspectorWidth + Prefs.minimapWidth + 75f, Prefs.maxMinimapHeight + 85f);

            if (windowInfo.inspectorToggled)
                window = new Rect(0f, 0f, position.width - Window.inspectorWidth - Window.padding * 2,
                    position.height);
            else
                window = new Rect(0f, 0f, position.width, position.height);

            RebuildComponentTree();

            wantsMouseMove = true;

            Undo.undoRedoPerformed += UndoPerformed;

            instance = this;

            if (target != null)
            {
                if (target.blackboard != null)
                    Blackboard.instance = target.blackboard;
                target.PurgeNull();
            }

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

            List<string> f = new(lastFiles);

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

        private static GUIContent AggregateErrors(List<Error> errors)
        {
            GUIContent ret = new();

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
            List<Error> errors = new(node.GetErrors());

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
    }
}