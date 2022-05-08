using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using Schema;
using Schema.Utilities;
using UnityEditor.ShortcutManagement;

namespace SchemaEditor
{
    //This class contains the basic information about the EditorWindow, including various preferences and basic methods (Delete, Select, etc.)
    public partial class NodeEditor : EditorWindow, IHasCustomMenu
    {
        public static NodeEditor instance;
        private static Dictionary<Type, List<Type>> nodeTypes;
        private static Type[] decoratorTypes;
        private static List<UnityEngine.Object> copyBuffer = new List<UnityEngine.Object>();
        public Node requestingConnection;
        private Node orphanNode;
        public Graph target;
        public Blackboard globalBlackboard;
        private Window windowInfo = new Window();
        private int nodeCount;
        [DidReloadScripts]
        static void Init()
        {
            nodeTypes = new Dictionary<Type, List<Type>>();
            foreach (Type t in HelperMethods.GetNodeTypes())
            {
                IEnumerable<Type> test = HelperMethods.GetEnumerableOfType(t);
                //Debug.Log(t + ": " + String.Join(",", test));
                nodeTypes.Add(t, test.ToList());
            }
            decoratorTypes = HelperMethods.GetEnumerableOfType(typeof(Decorator)).ToArray();
        }

        [MenuItem("Window/AI/Behavior Editor")]
        static void OpenWindow()
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
            windowInfo = new Window();
            windowInfo.editor = this;

            target = graphObj;
            windowInfo.zoom = target.zoom;
            windowInfo.pan = target.pan;

            if (graphObj == null)
            {
                titleContent = new GUIContent("Behavior Editor");
                return;
            }

            target.Initialize();

            titleContent = new GUIContent(graphObj.name);

            string id = AssetDatabase.GUIDFromAssetPath(AssetDatabase.GetAssetPath(target)).ToString();
            string last = EditorPrefs.GetString("Schema Recently Opened");
            List<string> lastFiles = last.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();

            if (lastFiles.Contains(id))
                lastFiles.MoveItemAtIndexToFront(lastFiles.IndexOf(id));
            else
                lastFiles.Insert(0, id);

            List<string> f = new List<string>(lastFiles);

            foreach (string s in f)
            {
                string path = AssetDatabase.GUIDToAssetPath(s);

                if (String.IsNullOrEmpty(path))
                {
                    lastFiles.Remove(s);
                    continue;
                }

                string absolutePath = Application.dataPath + path.Substring(6);

                if (!System.IO.File.Exists(absolutePath))
                    lastFiles.Remove(s);
            }

            EditorPrefs.SetString("Schema Recently Opened", String.Join(",", lastFiles));

            Selection.activeObject = target;

            Undo.ClearAll();

            Blackboard.instance = target.blackboard;
            GetViewRect(100f, true);
        }
        void IHasCustomMenu.AddItemsToMenu(GenericMenu menu)
        {
            menu.AddItem("Preferences", windowInfo.settingsShown, TogglePrefs, false);
            menu.AddItem("Documentation", false, () => OpenUrl("https://thinking-automation.vercel.app/docs/getting-started"), false);
        }
        void TogglePrefs()
        {
            windowInfo.inspectorToggled = !windowInfo.settingsShown;
            windowInfo.settingsShown = !windowInfo.settingsShown;
            windowInfo.inspectorScroll = Vector2.zero;
        }
        private static GUIContent AggregateErrors(List<Error> errors)
        {
            GUIContent ret = new GUIContent();

            if (errors.Count == 0) return GUIContent.none;

            Error.Severity severity = (Error.Severity)errors.Max(error => (int)error.severity);
            switch (severity)
            {
                case Error.Severity.Info:
                    ret.image = Styles.infoIcon;
                    break;
                case Error.Severity.Warning:
                    ret.image = Styles.warnIcon;
                    break;
                case Error.Severity.Error:
                    ret.image = Styles.errorIcon;
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
            else if (node.children.Length == 0 && node.CanHaveChildren()) errors.Add(new Error("No child node attatched", Error.Severity.Warning));

            return AggregateErrors(errors);
        }

        void UndoPerformed()
        {
            windowInfo.selected.RemoveAll(node => node == null);

            foreach (Node node in target.nodes)
                windowInfo.changedNodes.Enqueue(node);

            List<UnityEngine.Object> targets = new List<UnityEngine.Object>();

            if (windowInfo.selectedDecorator != null)
            {
                targets.Add(windowInfo.selectedDecorator);
            }
            else if (windowInfo.selected.Count > 0)
            {
                windowInfo.selected.ForEach(x => Debug.Log(x));
                targets.AddRange(windowInfo.selected);
            }

            distinctTypes = targets.Select(x => x.GetType()).Distinct().ToList();

            if (distinctTypes.Count > 1) return;

            UnityEditor.Editor.CreateCachedEditor(targets.ToArray(), null, ref editor);

            target.TraverseTree();
            GetViewRect(100f, true);
        }
        //Validates connections between nodes also resets HideFlags
        void OnEnable()
        {
            Undo.undoRedoPerformed += UndoPerformed;

            instance = this;
            if (target != null && target.blackboard != null)
                Blackboard.instance = target.blackboard;
        }
        void FocusSearch()
        {
            Debug.Log(searchWantsFocus);

            if (searchWantsFocus)
            {
                EditorGUI.FocusTextInControl("SearchTextField");
                Debug.Log("Hey");
                searchWantsFocus = false;
            }
        }
        void OnDestroy()
        {
            Undo.undoRedoPerformed -= UndoPerformed;
            Undo.ClearAll();

            foreach (SchemaAgent agent in GameObject.FindObjectsOfType<SchemaAgent>())
            {
                agent.editorTarget = null;
                SceneView.RepaintAll();
            }

            instance = null;
            Blackboard.instance = null;
            DestroyImmediate(editor);
            DestroyImmediate(blackboardEditor);
        }
        private void OpenUrl(string url)
        {
            try
            {
                System.Diagnostics.Process.Start(url);
            }
            catch
            {
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                }
                else if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.OSX))
                {
                    System.Diagnostics.Process.Start("xdg-open", url);
                }
                else if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.OSX))
                {
                    System.Diagnostics.Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
        }
        private void Select(Node node, bool add)
        {
            if (!add)
                windowInfo.selected.Clear();

            if (!windowInfo.selected.Contains(node))
            {
                windowInfo.selected.Add(node);
            }

            target.nodes.MoveItemAtIndexToFront(Array.IndexOf(target.nodes, node));

            EditorApplication.delayCall += () => SceneView.RepaintAll();
        }
        private void Select(Decorator decorator)
        {
            windowInfo.selectedDecorator = decorator;
            windowInfo.selected.Clear();

            EditorApplication.delayCall += () => SceneView.RepaintAll();
        }
        private Node[][] ConvertTo2DArray(List<Node> nodes)
        {
            Dictionary<string, int> parentCounts = nodes.ToDictionary(x => x.uID, x => GetParentCount(x));
            int rowCount = parentCounts.Values.Max() + 1;

            Node[][] arr = new Node[rowCount][];

            for (int i = 0; i < rowCount; i++)
            {
                List<Node> row = nodes.FindAll(node => parentCounts[node.uID] == i);
                row = row.OrderBy(node => node.priority).ToList();

                arr[i] = row.ToArray();
            }

            return arr;
        }
        private int GetParentCount(Node node)
        {
            int count = 0;
            Node current = node;

            while (current.parent != null)
            {
                current = current.parent;
                count++;
            }

            return count;
        }
        private bool IsLowerPriority(Node node, Node child)

        {
            int index = Array.IndexOf(node.parent.children, node);

            if (node == child)
                return true;
            if (index + 1 > node.parent.children.Length - 1)
                return false;

            return !IsSubTreeOf(node, child) && IsSubTreeOf(node.parent, child);
        }
        private bool IsSubTreeOf(Node node, Node child)
        {
            if (node == child)
                return true;

            if (child.priority < node.priority)
                return false;

            //Step up the tree until we reach the node, or a node with no parent
            Node current = child;
            while (current.parent != null)
            {
                current = current.parent;

                if (current == node)
                    return true;
            }

            return false;
        }

        ///<summary>
        ///Recalculates the list order of a node's children, based on position
        ///</summary>
        private void RecalculatePriorities(Node node)
        {
            IEnumerable<Node> prioritized = node.children.OrderBy(node =>
            {
                Vector2 nodeArea = GetArea(node, false);
                float x = (node.position.x + (nodeArea.x / 2f));
                return x;
            });

            // node.children = prioritized.ToArray();
        }
        private void Deselect(Node node)
        {
            if (windowInfo.selected.Contains(node))
            {
                windowInfo.selected.Remove(node);
            }

            SceneView.RepaintAll();
        }
        private void Deselect(Decorator decorator)
        {
            windowInfo.selectedDecorator = null;

            SceneView.RepaintAll();
        }
        private void DeleteSelected()
        {
            target.DeleteNodes(windowInfo.selected);

            target.TraverseTree();
            if (windowInfo.selected.Contains(windowInfo.hoveredNode))
                windowInfo.hoveredNode = null;
            windowInfo.selected.Clear();
        }
        private void Copy(List<Node> copies, bool clearSelected = true)
        {
            //Making copy list seprate from passed instance (caused issues when passing windowInfo.selected, which is modified in the Duplicate method)
            copies = new List<Node>(copies);

            List<Node> temp = new List<Node>();

            // Duplicate(copies, temp, false, clearSelected);

            copyBuffer.ForEach(obj =>
            {
                if ((typeof(Node).IsAssignableFrom(obj.GetType()) &&
                !target.nodes.Contains(obj)))
                {
                    DestroyImmediate(obj);
                }
                else if ((typeof(Decorator)).IsAssignableFrom(obj.GetType()) &&
                !target.nodes.Any(node => node.decorators.Contains(obj)))
                {
                    DestroyImmediate(obj);
                }
            });
            copyBuffer.Clear();

            copyBuffer.AddRange(temp);
        }
        private void Copy(Decorator decorator, bool clearSelected = true)
        {
            Decorator instance = ScriptableObject.Instantiate(decorator);

            if (clearSelected)
                windowInfo.selected.Clear();

            copyBuffer.ForEach(obj =>
            {
                if ((typeof(Node).IsAssignableFrom(obj.GetType()) &&
                !target.nodes.Contains(obj)))
                {
                    DestroyImmediate(obj);
                }
                else if ((typeof(Decorator)).IsAssignableFrom(obj.GetType()) &&
                !target.nodes.Any(node => node.decorators.Contains(obj)))
                {
                    DestroyImmediate(obj);
                }
            });
            copyBuffer.Clear();
            copyBuffer.Add(decorator);
        }
        private void Paste()
        {

            // if (copyBuffer.OfType<Node>().Count() > 0)
            // {
            //     Undo.RegisterCompleteObjectUndo(target, "Paste Nodes");

            //     windowInfo.selected.Clear();
            //     List<Node> nodes = copyBuffer.OfType<Node>().ToList();

            //     copyBuffer.ForEach(x => Debug.Log(x));

            //     foreach (Node node in nodes)
            //     {
            //         target.nodes.Add(node);
            //         Select(node, true);
            //     }
            //     target.TraverseTree();
            //     //Re-copy copy buffer so nodes references shared after first paste
            //     Copy(nodes, false);
            // }
            // else if (copyBuffer.OfType<Decorator>().Count() > 0)
            // {
            //     Undo.RegisterCompleteObjectUndo(target, "Paste Decorator");

            //     Decorator decorator = copyBuffer.OfType<Decorator>().ElementAt(0);

            //     foreach (Node node in windowInfo.selected)
            //     {
            //         Decorator instance = ScriptableObject.Instantiate(decorator);

            //         instance.name = decorator.name;

            //         instance.uID = Guid.NewGuid().ToString("N");

            //         ArrayUtility.Add(ref node.decorators, instance);
            //         instance.node = node;
            //     }
            // }
        }
        private void AddNode(Type nodeType, Vector2 position, bool asChild)
        {
            if (!typeof(Node).IsAssignableFrom(nodeType))
                return;

            target.AddNode(nodeType, position);

            // List<Node> operators = new List<Node>(windowInfo.selected);
            // operators = operators.OrderBy(node => node.priority).ToList();

            // windowInfo.selected.Clear();


            // for (int i = 0; i < (asChild ? operators.Count : 1); i++)
            // {
            //     if (asChild && !operators[i].canHaveChildren) continue;

            //     Node node = (Node)ScriptableObject.CreateInstance(nodeType);
            //     node.name = node.GetType().Name;
            //     node.hideFlags = HideFlags.HideAndDontSave;
            //     List<UnityEngine.Object> toRecord = new List<UnityEngine.Object> { node };
            //     if (node.parent != null) toRecord.Add(node.parent);
            //     toRecord.AddRange(node.children);

            //     if (asChild)
            //         toRecord.AddRange(operators);

            //     //Add the node
            //     Undo.RecordObjects(toRecord.ToArray(), "Add Node");
            //     //So order changes do not cause errors
            //     Undo.RegisterCompleteObjectUndo(target, "Add Node");
            //     node.WorkingNode();
            //     target.nodes.Add(node);
            //     node.graph = target;

            //     Vector2 size = GetArea(node, false);

            //     if (!asChild)
            //     {
            //         node.position = position - GetAreaWithPadding(node, false) / 2f;
            //     }
            //     else
            //     {
            //         if (operators[i].children.Count > 0)
            //         {
            //             Node farthest = operators[i].children[operators[i].children.Count - 1];
            //             Vector2 farthestSize = GetArea(farthest, false);

            //             node.position = new Vector2(farthest.position.x + farthestSize.x + 50f, farthest.position.y);
            //         }
            //         else
            //         {
            //             Vector2 parentSize = GetArea(operators[i], false);

            //             node.position = new Vector2(operators[i].position.x + parentSize.x / 2f - size.x / 2f, operators[i].position.y + parentSize.y + 150f);
            //         }
            //     }

            //     if (asChild)
            //         AddConnection(operators[i], node, false);

            //     Select(node, true);
            // }

            target.TraverseTree();
            GetViewRect(100f, true);
        }
        private void MoveDecoratorInNode(Decorator d, bool up)
        {
            if (d == null) return;

            Undo.IncrementCurrentGroup();
            int groupIndex = Undo.GetCurrentGroup();

            string name = String.Format("Move Decorator {0}", up ? "Up" : "Down");

            Undo.RegisterCompleteObjectUndo(d.node, name);
            Undo.RecordObject(d, name);

            int decoratorIndex = Array.IndexOf(d.node.decorators, d);
            d.node.decorators.Swap(decoratorIndex, up ? decoratorIndex - 1 : decoratorIndex + 1);
        }
        private void MoveDecoratorInNode(Decorator d, int index)
        {
            if (d == null || index < 0 || index > d.node.decorators.Length) return;
            Debug.Log("Continuted");

            Undo.IncrementCurrentGroup();
            int groupIndex = Undo.GetCurrentGroup();

            string name = String.Format("Move Decorator To Index {0}", index);

            Undo.RegisterCompleteObjectUndo(d.node, name);
            Undo.RecordObject(d, name);

            int decoratorIndex = Array.IndexOf(d.node.decorators, d);
            d.node.decorators.Move(d, index);
        }
        // private void AddConnection(Node n1, Node n2, bool recordUndo)
        // {
        //     if (recordUndo)
        //         Undo.RecordObjects(new UnityEngine.Object[] { n1, n2 }, "Add Connection");
        //     if (!n1.children.Contains(n2))
        //         ArrayUtility.Add(ref n1.children, n2);
        //     n2.parent = n1;
        //     orphanNode = null;
        // }

        // private void RemoveConnection(Node n1, Node n2)
        // {
        //     Undo.RecordObjects(new UnityEngine.Object[] { n1, n2 }, "Remove Connection");
        //     ArrayUtility.Remove(ref n1.children, n2);
        //     n2.parent = null;
        //     target.TraverseTree();
        //     GetViewRect(100f, true);
        // }

        public void Duplicate()
        {
            List<Node> selected = new List<Node>(windowInfo.selected);

            windowInfo.selected.Clear();

            foreach (Node node in target.Duplicate(selected, Vector2.one * 50f, true))
                Select(node, true);
        }
        private Node GetSiblingNode(Node node, bool left)
        {
            if (node == null || node.parent == null)
                return null;

            Node parent = node.parent;
            int thisIndex = Array.IndexOf(node.parent.children, node);

            if (left && thisIndex == 0)
                return null;
            else if (!left && thisIndex == parent.children.Length - 1)
                return null;

            return parent.children[left ? thisIndex - 1 : thisIndex + 1];
        }
        internal GenericMenu GenerateContextMenu()
        {
            GenericMenu g = new GenericMenu();

            switch (windowInfo.hoveredType)
            {
                case Window.Hovering.Node:
                    g.AddItem(GenerateMenuItem("Main Menu/Edit/Cut"), false, () => { }, editingPaused);
                    g.AddItem(GenerateMenuItem("Main Menu/Edit/Copy"), false, () => Copy(windowInfo.selected), editingPaused);
                    g.AddItem(GenerateMenuItem("Main Menu/Edit/Paste"), false, () => Paste(), editingPaused || copyBuffer.Count == 0);
                    g.AddItem(GenerateMenuItem("Main Menu/Edit/Delete"), false, () => DeleteSelected(), editingPaused);
                    g.AddItem("Break Connections %&b", false, () => target.BreakConnections(windowInfo.selected), editingPaused);
                    g.AddSeparator("");
                    g.AddItem(GenerateMenuItem("Schema/Add Decorator"), false, () => AddDecoratorCommand(), editingPaused);
                    break;
                case Window.Hovering.InConnection:
                    g.AddItem("Break Connection", false, () => windowInfo.hoveredNode.RemoveParent(), editingPaused);
                    break;
                case Window.Hovering.OutConnection:
                    g.AddItem("Break Connection", false, () => windowInfo.hoveredNode.RemoveChildren(), editingPaused);
                    break;
                case Window.Hovering.Decorator:
                    g.AddItem(GenerateMenuItem("Main Menu/Edit/Cut"), false, () => { }, editingPaused);
                    g.AddItem(GenerateMenuItem("Main Menu/Edit/Copy"), false, () => { }, editingPaused);
                    g.AddItem(GenerateMenuItem("Main Menu/Edit/Paste"), false, () => { }, editingPaused || copyBuffer.Count == 0);
                    g.AddItem(GenerateMenuItem("Main Menu/Edit/Delete"), false, () =>
                    {
                        Debug.Log(windowInfo.hoveredDecorator);
                        windowInfo.hoveredDecorator.node.RemoveDecorator(windowInfo.hoveredDecorator);
                    }, editingPaused);
                    g.AddItem("Move Up %UP", false, () => MoveUpCommand(), editingPaused);
                    g.AddItem("Move Down %DOWN", false, () => MoveDownCommand(), editingPaused);
                    break;
                case Window.Hovering.Window:
                    g.AddItem(GenerateMenuItem("Schema/Add Node"), false, () => AddNodeCommand(), editingPaused);
                    g.AddItem(GenerateMenuItem("Main Menu/Edit/Select All"), false, () =>
                            {
                                List<Node> iteration = new List<Node>(target.nodes);

                                foreach (Node node in iteration)
                                {
                                    Select(node, true);
                                }
                            }, editingPaused);
                    g.AddItem(GenerateMenuItem("Main Menu/Edit/Deselect All"), false, () => windowInfo.selected.Clear(), editingPaused);
                    g.AddSeparator("");
                    g.AddItem(GenerateMenuItem("Main Menu/Edit/Frame Selected", windowInfo.selected.Count > 0 ? "" : "Focus Root"), false, () =>
                    {
                        if (windowInfo.selected.Count > 0)
                        {
                            List<Vector2> positions = windowInfo.selected.Select(node => node.position).ToList();

                            float x = 0f;
                            float y = 0f;

                            foreach (Vector2 pos in positions)
                            {
                                Vector2 area = GetArea(windowInfo.selected[positions.IndexOf(pos)], false);

                                x += pos.x + area.x / 2f;
                                y += pos.y + area.y / 2f;
                            }

                            Vector2 avg = new Vector2(x / windowInfo.selected.Count, y / windowInfo.selected.Count);

                            PanView(-avg, 1f);
                        }
                        else
                        {
                            PanView(-(target.root.position + GetArea(target.root, false) / 2f), 1f);
                        }
                    }, editingPaused);
                    g.AddItem("Zoom In", false, () => windowInfo.zoom -= 3 * GUIData.zoomSpeed, false);
                    g.AddItem("Zoom Out", false, () => windowInfo.zoom += 3 * GUIData.zoomSpeed, false);
                    g.AddSeparator("");
                    g.AddItem(GenerateMenuItem("Main Menu/Edit/Paste"), false, () => Paste(), editingPaused || copyBuffer.Count == 0);
                    g.AddItem(GenerateMenuItem("Main Menu/Edit/Undo"), false, () => Undo.PerformUndo(), editingPaused);
                    g.AddItem(GenerateMenuItem("Main Menu/Edit/Redo"), false, () => Undo.PerformRedo(), editingPaused);
                    break;
                case Window.Hovering.Inspector:
                case Window.Hovering.None:
                    break;
            }

            return g;
        }


        internal GenericMenu GenerateNodeContextMenu()
        {
            GenericMenu g = new GenericMenu();
            g.AddItem(GenerateMenuItem("Main Menu/Edit/Cut"), false, () => { }, editingPaused);
            g.AddItem(GenerateMenuItem("Main Menu/Edit/Copy"), false, () => Copy(windowInfo.selected), editingPaused);
            g.AddItem(GenerateMenuItem("Main Menu/Edit/Paste"), false, () => Paste(), editingPaused || copyBuffer.Count == 0);
            g.AddItem(GenerateMenuItem("Main Menu/Edit/Delete"), false, () => DeleteSelected(), editingPaused);
            g.AddItem("Break Connections %b", false, () => target.BreakConnections(windowInfo.selected), editingPaused);
            g.AddSeparator("");
            g.AddItem(GenerateMenuItem("Schema/Add Decorator"), false, () => AddDecoratorCommand(), editingPaused);
            // g.AddItem(GenerateMenuItem("Schema/Add Child"), false, () => AddChildCommand(), !windowInfo.hoveredNode.canHaveChildren);

            return g;
        }
        string GenerateMenuItem(string commandName, string overrideName = "")
        {
            int pos = commandName.LastIndexOf("/") + 1;
            string menuName = String.IsNullOrEmpty(overrideName) ? commandName.Substring(pos) : overrideName;

            KeyCombination def = GetCommandKeyCombination(commandName);
            string keyCode = GetMenuKeyFromKeyCode(def.keyCode);

            if (!String.IsNullOrEmpty(keyCode))
            {
                if (def.action || def.alt || def.shift)
                {
                    menuName += $" {(def.action ? "%" : "")}{(def.shift ? "#" : "")}{(def.alt ? "&" : "")}{keyCode}";
                }
                else
                {
                    menuName += $" _{keyCode}";
                }
            }

            return menuName;
        }
        KeyCombination GetCommandKeyCombination(string commandName)
        {
            IEnumerable<KeyCombination> sequence =
                UnityEditor.ShortcutManagement.ShortcutManager.instance.GetShortcutBinding(commandName).keyCombinationSequence;

            KeyCombination defaultKeyCombination = new KeyCombination(KeyCode.None);

            if (sequence.Count() > 0)
                return UnityEditor.ShortcutManagement.ShortcutManager.instance.GetShortcutBinding(commandName).keyCombinationSequence.ElementAt(0);
            else
                return defaultKeyCombination;
        }
        string GetMenuKeyFromKeyCode(KeyCode code)
        {
            //Return the raw keycode if keycode is a letter
            string codeStr = code.ToString();
            if (System.Text.RegularExpressions.Regex.IsMatch(codeStr, @"^[a-zA-Z]$"))
            {
                return codeStr.ToLower();
            }

            //Otherwise, manually return keycodes
            switch (code)
            {
                case KeyCode.None:
                    return "";
                case KeyCode.Home:
                    return "HOME";
                case KeyCode.PageUp:
                    return "PGUP";
                case KeyCode.PageDown:
                    return "PGDN";
                case KeyCode.End:
                    return "END";
                case KeyCode.UpArrow:
                    return "UP";
                case KeyCode.LeftArrow:
                    return "LEFT";
                case KeyCode.RightArrow:
                    return "RIGHT";
                case KeyCode.DownArrow:
                    return "DOWN";
                case KeyCode.Insert:
                    return "INS";
                case KeyCode.Delete:
                    return "DEL";
                case KeyCode.Alpha0:
                case KeyCode.Alpha1:
                case KeyCode.Alpha2:
                case KeyCode.Alpha3:
                case KeyCode.Alpha4:
                case KeyCode.Alpha5:
                case KeyCode.Alpha6:
                case KeyCode.Alpha7:
                case KeyCode.Alpha8:
                case KeyCode.Alpha9:
                    return codeStr.Substring(5);
                case KeyCode.Backslash:
                    return "\\";
                case KeyCode.Slash:
                    return "/";
                case KeyCode.Period:
                    return ".";
                case KeyCode.BackQuote:
                    return "`";
                case KeyCode.LeftBracket:
                    return "[";
                case KeyCode.RightBracket:
                    return "]";
                case KeyCode.Minus:
                    return "-";
                case KeyCode.Equals:
                    return "=";
                case KeyCode.Semicolon:
                    return ";";
                case KeyCode.Quote:
                    return "\'";
                case KeyCode.Comma:
                    return ",";
                case KeyCode.F1:
                case KeyCode.F2:
                case KeyCode.F3:
                case KeyCode.F4:
                case KeyCode.F5:
                case KeyCode.F6:
                case KeyCode.F7:
                case KeyCode.F8:
                case KeyCode.F9:
                case KeyCode.F10:
                case KeyCode.F11:
                case KeyCode.F12:
                case KeyCode.F13:
                case KeyCode.F14:
                case KeyCode.F15:
                    return codeStr.Substring(2);
            }

            //Cases where nothing is rendered to the menu
            return "";
        }
        ///<summary>
        ///Sets the cursor of the window
        ///</summary>
        public void SetCursor(MouseCursor cursor)
        {
            EditorGUIUtility.AddCursorRect(new Rect(0, 0, position.width, position.height), cursor);
        }

        //These are methods that transform a position into another position with zoom and pan accounted for. Taken from the excellent XNode framework
        public Vector2 WindowToGridPosition(Vector2 windowPosition)
        {
            return (windowPosition - (window.size * 0.5f) - (windowInfo.pan / windowInfo.zoom)) * windowInfo.zoom;
        }
        public Rect WindowToGridRect(Rect windowRect)
        {
            windowRect.position = WindowToGridPosition(windowRect.position);
            windowRect.size *= windowInfo.zoom;
            return windowRect;
        }

        private Vector2 GridToWindowPosition(Vector2 gridPosition)
        {
            return (window.size * 0.5f) + (windowInfo.pan / windowInfo.zoom) + (gridPosition / windowInfo.zoom);
        }

        public Rect GridToWindowRectNoClipped(Rect gridRect)
        {
            gridRect.position = GridToWindowPositionNoClipped(gridRect.position);
            return gridRect;
        }

        public Rect GridToWindowRect(Rect gridRect)
        {
            gridRect.position = GridToWindowPosition(gridRect.position);
            gridRect.size /= windowInfo.zoom;
            return gridRect;
        }
        public Vector2 GridToMinimapPosition(Vector2 gridPosition, float width, float padding)
        {
            Rect r = GetViewRect(padding, false);

            Vector2 position = (gridPosition - r.position) / r.size;
            Vector2 sizeFac = new Vector2(width, r.height / r.width * width);

            return position * sizeFac;
        }
        public Rect GetViewRect(float padding, bool recalculate)
        {
            if (recalculate)
            {
                nodeCount = target.nodes.Length;

                float xMax = target.nodes.Max(node => node.position.x + GetAreaWithPadding(node, false).x);
                float xMin = target.nodes.Min(node => node.position.x);
                float yMax = target.nodes.Max(node => node.position.y + GetAreaWithPadding(node, false).y);
                float yMin = target.nodes.Min(node => node.position.y);

                windowInfo.viewRect = new Rect(xMin - padding, yMin - padding, xMax - xMin + padding * 2f, yMax - yMin + padding * 2f);
            }

            return windowInfo.viewRect;
        }
        public Vector2 GridToWindowPositionNoClipped(Vector2 gridPosition)
        {
            Vector2 center = window.size * 0.5f;
            // UI Sharpness complete fix - Round final offset not panOffset
            float xOffset = center.x * windowInfo.zoom + (windowInfo.pan.x + gridPosition.x);
            float yOffset = center.y * windowInfo.zoom + (windowInfo.pan.y + gridPosition.y);
            return new Vector2(xOffset, yOffset);
        }
        [OnOpenAsset(1)]
        public static bool Open(int instanceID, int line)
        {
            UnityEngine.Object obj = EditorUtility.InstanceIDToObject(instanceID);
            if (obj as Graph != null)
            {
                NodeEditor.OpenGraph((Graph)obj);
                return true;
            }
            else
            {
                return false;
            }
        }
        //--SCHEMA SHORTCUTS--//
        [Shortcut("Schema/Add Node", KeyCode.A, ShortcutModifiers.Shift)]
        private static void AddNodeCommand()
        {
            if (instance == null) return;

            instance.windowInfo.searchWantsNode = true;
            instance.windowInfo.searchAddChildren = false;

            if (!instance.editingPaused)
                instance.ToggleSearch();
        }
        [Shortcut("Schema/Add Child", KeyCode.N, ShortcutModifiers.Action | ShortcutModifiers.Alt)]
        private static void AddChildCommand()
        {
            // if (instance == null || instance.windowInfo.selected.Count == 0 || !instance.windowInfo.selected[0].canHaveChildren) return;

            instance.windowInfo.searchWantsNode = true;
            instance.windowInfo.searchAddChildren = true;

            if (!instance.editingPaused)
                instance.ToggleSearch();
        }
        [Shortcut("Schema/Add Decorator", KeyCode.D, ShortcutModifiers.Action | ShortcutModifiers.Alt)]
        private static void AddDecoratorCommand()
        {
            if (instance == null || instance.windowInfo.selected.Count == 0) return;

            instance.windowInfo.searchWantsNode = false;

            if (!instance.editingPaused)
                instance.ToggleSearch();
        }
        [Shortcut("Schema/Move Up", KeyCode.UpArrow, ShortcutModifiers.Action)]
        private static void MoveUpCommand()
        {
            if (instance == null || instance.windowInfo.selectedDecorator == null) return;

            instance.MoveDecoratorInNode(instance.windowInfo.selectedDecorator, true);
        }
        [Shortcut("Schema/Move Down", KeyCode.DownArrow, ShortcutModifiers.Action)]
        private static void MoveDownCommand()
        {
            if (instance == null || instance.windowInfo.selectedDecorator == null) return;

            instance.MoveDecoratorInNode(instance.windowInfo.selectedDecorator, false);
        }
        [Shortcut("Schema/Test", KeyCode.J, ShortcutModifiers.Action | ShortcutModifiers.Shift)]
        private static void TestCommand()
        {
            DynamicPropertyBuilder.Build();
        }
        [Shortcut("Schema/Break Connections", KeyCode.B, ShortcutModifiers.Action | ShortcutModifiers.Alt)]
        private static void BreakConnectionsCommand()
        {
            if (instance == null) return;

            instance.target.BreakConnections(instance.windowInfo.selected);
        }
        internal static class NodeEditorPrefs
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
                EditorPrefs.SetString(key, String.Join(",", values));
            }
            public static void ResetToDefault()
            {
                List<TypeCode> valid = new List<TypeCode> { TypeCode.String, TypeCode.Single, TypeCode.Boolean, TypeCode.Int32 };

                Type t = typeof(NodeEditorPrefs);
                PropertyInfo[] fields = t.GetProperties(BindingFlags.Static | BindingFlags.Public);

                foreach (PropertyInfo property in fields)
                {
                    if (valid.Contains(Type.GetTypeCode(property.PropertyType)))
                    {
                        EditorPrefs.DeleteKey("SCHEMA_PREF__" + property.Name);
                    }
                    else if (typeof(UnityEngine.Color).IsAssignableFrom(property.PropertyType))
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
}