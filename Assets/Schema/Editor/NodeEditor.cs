using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using Schema.Runtime;
using Schema.Utilities;
using UnityEditor.ShortcutManagement;

namespace Schema.Editor
{
    //This class contains the basic information about the EditorWindow, including various preferences and basic methods (Delete, Select, etc.)
    public partial class NodeEditor : EditorWindow, IHasCustomMenu
    {
        private static NodeEditor instance;
        private static Dictionary<Type, List<Type>> nodeTypes;
        private static Type[] decoratorTypes;
        private static List<UnityEngine.Object> copyBuffer = new List<UnityEngine.Object>();
        public Node requestingConnection;
        private Node orphanNode;
        public Graph original;
        public Graph target;
        public Window windowInfo;
        public GUIData guiData;
        private int nodeCount;
        private int jk;
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
            windowInfo.dividerPos = position.height / 2f;
            guiData = new GUIData();
            guiData.sizes = new SerializableDictionary<Node, Vector2>();

            windowInfo.selected = new List<Node>();

            if (graphObj == null)
            {
                titleContent = new GUIContent("Behavior Editor");
                return;
            }

            titleContent = new GUIContent(graphObj.name);
            this.Load(graphObj);

            string id = AssetDatabase.GUIDFromAssetPath(AssetDatabase.GetAssetPath(original)).ToString();
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

            if (target.root == null)
            {
                AddNode(typeof(Root), Vector2.zero, false);
                target.root = (Root)target.nodes.Find(node => node.GetType() == typeof(Root));
                target.root.graph = target;
                TraverseTree();
            }

            Undo.ClearAll();

            UpdateSelectors();
            Blackboard.instance = target.blackboard;
            GetViewRect(100f, true);
        }
        void IHasCustomMenu.AddItemsToMenu(GenericMenu menu)
        {
            menu.AddItem("Preferences", windowInfo.settingsShown, () => TogglePrefs(), false);
            menu.AddItem("Documentation", false, () => OpenUrl("https://www.google.com"), false);
        }
        void TogglePrefs()
        {
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
                    ret.image = NodeEditorResources.infoIcon;
                    break;
                case Error.Severity.Warning:
                    ret.image = NodeEditorResources.warnIcon;
                    break;
                case Error.Severity.Error:
                    ret.image = NodeEditorResources.errorIcon;
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

            foreach (Decorator d in node.decorators)
                errors.AddRange(d.GetErrors().Select(error => new Error($"{error.message} ({d.Name})", error.severity)));

            if (node.priority < 1) errors.Add(new Error("Node not connected to root!", Error.Severity.Warning));
            else if (node.children.Count == 0 && node.canHaveChildren) errors.Add(new Error("No child node attatched", Error.Severity.Warning));

            return AggregateErrors(errors);
        }

        void UpdateSelectors()
        {
            List<BlackboardEntrySelector> toConnect = new List<BlackboardEntrySelector>();

            //Connect all entry selectors here
            foreach (Node node in target.nodes)
            {
                toConnect.AddRange(GetSelectors(node));

                foreach (Decorator d in node.decorators)
                {
                    Debug.Log("Running");
                    toConnect.AddRange(GetSelectors(d));
                }
            }

            foreach (BlackboardEntrySelector s in toConnect)
            {
                target.blackboard.ConnectSelector(s);
            }

        }
        IEnumerable<BlackboardEntrySelector> GetSelectors(object obj)
        {
            jk++;

            if (jk > 1000) yield break;

            HashSet<Type> nonRecursiveTypes = new HashSet<Type> {
                typeof(sbyte),
                typeof(byte),
                typeof(short),
                typeof(ushort),
                typeof(int),
                typeof(uint),
                typeof(long),
                typeof(ulong),
                typeof(char),
                typeof(float),
                typeof(double),
                typeof(decimal),
                typeof(bool),
                typeof(string),
                typeof(Enum)
            };

            HashSet<string> invalidNames = new HashSet<string> {
                "parent",
                "children",
                "decorators",
        		//! This will cause the editor to crash if removed
        		"graph",
                "position",
                "node",
                "_icon"
            };

            if (obj == null) yield break;

            foreach (FieldInfo field in obj.GetType().GetFields())
            {
                if (!nonRecursiveTypes.Any(t => t.IsAssignableFrom(field.FieldType)) && !invalidNames.Contains(field.Name))
                {
                    if (typeof(BlackboardEntrySelector).IsAssignableFrom(field.FieldType))
                    {
                        yield return (BlackboardEntrySelector)field.GetValue(obj);
                    }
                    else if (typeof(IEnumerable<BlackboardEntrySelector>).IsAssignableFrom(field.FieldType))
                    {
                        IEnumerable<BlackboardEntrySelector> l = (IEnumerable<BlackboardEntrySelector>)field.GetValue(obj);

                        if (l != null)
                        {
                            foreach (BlackboardEntrySelector s in l)
                                yield return s;
                        }
                    }
                    else
                    {
                        foreach (BlackboardEntrySelector s in GetSelectors(field.GetValue(obj)))
                            yield return s;
                    }
                }
            }
        }
        void UndoPerformed()
        {
            windowInfo.treeDirty = true;
            windowInfo.selected.RemoveAll(node => node == null);

            //set all nodes to dirty to recalculate sizes
            foreach (Node node in target.nodes)
            {
                Debug.Log(node.Name);
                node.dirty = true;
            }

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

            ValidateConnections();
            TraverseTree();
            GetViewRect(100f, true);
        }
        //Validates connections between nodes also resets HideFlags
        void ValidateConnections()
        {
            foreach (Node node in target.nodes)
            {
                if (node.parent != null && !node.parent.children.Contains(node))
                    node.parent.children.Add(node);

                foreach (Node child in node.children)
                {
                    child.parent = node;
                }

                node.hideFlags = HideFlags.HideAndDontSave;
            }
        }
        void UnloadChanges(PlayModeStateChange state)
        {
            //About to exit playmode, load saved file
            if (!EditorApplication.isPlayingOrWillChangePlaymode &&
     EditorApplication.isPlaying)
            {
                this.Load(original);
            }
            //About to enter playmode, save file
            else if (EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isPlaying)
            {
                windowInfo.graphSaved = true;
                titleContent = new GUIContent(original.name);
                NodeEditorFileHandler.Save(this);
            }
        }
        void OnEnable()
        {
            Undo.undoRedoPerformed += UndoPerformed;
            EditorApplication.playModeStateChanged += UnloadChanges;
            UnityEditor.ShortcutManagement.ShortcutManager.instance.activeProfileChanged += ResetShortcuts;
            RegisterShortcuts();

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
            if (NodeEditorPrefs.saveOnClose)
                NodeEditorFileHandler.Save(this);

            Undo.undoRedoPerformed -= UndoPerformed;
            EditorApplication.playModeStateChanged -= UnloadChanges;
            UnityEditor.ShortcutManagement.ShortcutManager.instance.activeProfileChanged -= ResetShortcuts;
            Undo.ClearAll();

            if (target)
            {
                DestroyImmediate(target.blackboard);
                DestroyImmediate(target);
            }

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

            target.nodes.MoveItemAtIndexToFront(target.nodes.IndexOf(node));

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
        //This is slow, clunky, and confusing.
        public void BeautifyTree(Vector2 spacing)
        {
            Node root = target.nodes.Find(node => node.GetType() == typeof(Root));
            IEnumerable<Node> nodes = target.nodes.FindAll(node => IsSubTreeOf(root, node));

            Node[][] arr = ConvertTo2DArray(nodes.ToList());

            List<Node> alignToParent = new List<Node>();
            List<Node> alignToChildren = new List<Node>();

            float y = 0f;
            //Initial sorting step
            foreach (Node[] row in arr)
            {
                float prevSize = 0f;
                float height = 0f;
                for (int i = 0; i < row.Length; i++)
                {
                    Node node = row[i];

                    float x = prevSize + spacing.x * i;

                    Vector2 area = GetArea(node, false);

                    node.position = new Vector2(x, y);

                    prevSize += area.x;
                    height = Mathf.Max(area.y, height);
                }
                y += height + spacing.y;

                foreach (Node node in row)
                {
                    if (node.parent == null || node.priority - node.parent.priority != 1)
                        continue;

                    float diff = node.position.x - (node.parent.position.x + GetArea(node.parent, false).x / 2f - GetArea(node, false).x / 2f);

                    if (Mathf.Abs(diff) > 0.01f)
                        alignToParent.Add(node);
                }
            }

            Dictionary<Node, float> translated = new Dictionary<Node, float>();

            int j = 0;
            foreach (Node[] row in arr.Reverse())
            {
                List<Node> rowList = row.ToList();

                foreach (Node node in row)
                {
                    if (alignToParent.Contains(node) || alignToChildren.Contains(node))
                    {
                        float diff = 0f;
                        List<Node> exclude = new List<Node>();

                        if (alignToChildren.Contains(node))
                        {
                            if (translated.ContainsKey(node))
                            {
                                MoveRow(
                                    rowList,
                                    new Vector2(node.position.x + translated[node], node.position.y),
                                    rowList.IndexOf(node),
                                    new List<Node>()
                                );
                                translated.Remove(node);
                            }


                            diff = node.position.x - (node.children[0].position.x + GetArea(node.children[0], false).x / 2f - GetArea(node, false).x / 2f);
                            alignToChildren.Remove(node);
                            exclude.AddRange(node.children);

                            MoveRow(
                                rowList,
                                new Vector2(node.position.x - diff, node.position.y),
                                rowList.IndexOf(node),
                                exclude
                            );

                            foreach (Node n in rowList.Skip(rowList.IndexOf(node)).ToList().FindAll(node => !translated.ContainsKey(node)))
                                translated.Add(n, diff);
                        }

                        if (alignToParent.Contains(node))
                        {
                            if (translated.ContainsKey(node))
                                translated.Remove(node);

                            diff = node.position.x - (node.parent.position.x + GetArea(node.parent, false).x / 2f - GetArea(node, false).x / 2f);
                            alignToParent.Remove(node);

                            if (diff > 0f)
                            {
                                alignToChildren.Add(node.parent);
                                continue;
                            }

                            MoveRow(
                                rowList,
                                new Vector2(node.position.x - diff, node.position.y),
                                rowList.IndexOf(node),
                                exclude
                            );
                        }
                    }
                }

                j++;
            }

            windowInfo.treeDirty = true;
            GetViewRect(100f, true);
        }
        private void MoveRow(List<Node> row, Vector2 position, int startIndex, List<Node> exclude)
        {
            List<Node> rowModified = row.Skip(startIndex).ToList();
            Vector2 diff = rowModified[0].position - position;

            foreach (Node node in rowModified)
            {
                if (!exclude.Contains(node))
                    MoveNodeAndChildren(node, node.position - diff, exclude);
            }
        }
        private void MoveNodeAndChildren(Node node, Vector2 pos, List<Node> exclude)
        {
            Vector2 diff = node.position - pos;
            node.position -= diff;
            foreach (Node child in node.children)
            {
                if (!exclude.Contains(child))
                    MoveNodeAndChildren(child, child.position - diff, exclude);
            }
        }
        private void TraverseTree()
        {
            foreach (Node node in target.nodes) node.priority = 0;

            if (target.root == null) return;

            TraverseSubtree(target.root, 1);
        }

        private int TraverseSubtree(Node node, int i)
        {
            node.priority = i;
            int children = 0;
            foreach (Node child in node.children)
            {
                int j = TraverseSubtree(child, i + 1);
                children += j + 1;
                i += j + 1;
            }
            return children;
        }
        private bool IsLowerPriority(Node node, Node child)

        {
            int index = node.parent.children.IndexOf(node);

            if (node == child)
                return true;
            if (index + 1 > node.parent.children.Count - 1)
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

            node.children = prioritized.ToList();
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
        private void Delete(Decorator decorator)
        {
            Undo.RecordObjects(new UnityEngine.Object[] { decorator.node, decorator }, "Delete Decorator");
            Undo.RegisterCompleteObjectUndo(target, "Delete Decorator");

            decorator.node.decorators.Remove(decorator);

            decorator.node.dirty = true;
        }
        private void Delete(Node node)
        {
            List<UnityEngine.Object> toRecord = new List<UnityEngine.Object> { node };
            if (node.parent != null) toRecord.Add(node.parent);
            toRecord.AddRange(node.children);
            // Remove the node
            Undo.RecordObjects(toRecord.ToArray(), "Delete Node");
            //so order changes will not cause errors
            Undo.RegisterCompleteObjectUndo(target, "Delete Node");
            target.RemoveNode(node);

            if (node.parent != null)
                node.parent.children.Remove(node);

            foreach (Node child in node.children)
                child.parent = null;

            GetViewRect(100f, true);
        }

        private void DeleteSelected()
        {
            windowInfo.treeDirty = true;

            Node[] toDelete = windowInfo.selected.FindAll(node => !node.GetType().Equals(typeof(Root))).ToArray();
            windowInfo.selected.RemoveAll(node => !node.GetType().Equals(typeof(Root)));

            foreach (Node node in toDelete) Delete(node);

            Undo.CollapseUndoOperations(Undo.GetCurrentGroup());

            TraverseTree();
        }
        private void Copy(List<Node> copies, bool clearSelected = true)
        {
            //Making copy list seprate from passed instance (caused issues when passing windowInfo.selected, which is modified in the Duplicate method)
            copies = new List<Node>(copies);

            List<Node> temp = new List<Node>();

            Duplicate(copies, temp, false, clearSelected);

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
            windowInfo.treeDirty = true;

            if (copyBuffer.OfType<Node>().Count() > 0)
            {
                Undo.RegisterCompleteObjectUndo(target, "Paste Nodes");

                windowInfo.selected.Clear();
                List<Node> nodes = copyBuffer.OfType<Node>().ToList();

                copyBuffer.ForEach(x => Debug.Log(x));

                foreach (Node node in nodes)
                {
                    target.nodes.Add(node);
                    Select(node, true);
                }
                TraverseTree();
                //Re-copy copy buffer so nodes references shared after first paste
                Copy(nodes, false);
            }
            else if (copyBuffer.OfType<Decorator>().Count() > 0)
            {
                Undo.RegisterCompleteObjectUndo(target, "Paste Decorator");

                Decorator decorator = copyBuffer.OfType<Decorator>().ElementAt(0);

                foreach (Node node in windowInfo.selected)
                {
                    Decorator instance = ScriptableObject.Instantiate(decorator);

                    instance.name = decorator.name;

                    instance.uID = Guid.NewGuid().ToString("N");

                    node.decorators.Add(instance);
                    instance.node = node;
                }
            }

            UpdateSelectors();
        }
        private void AddNode(Type nodeType, Vector2 position, bool asChild)
        {
            if (!typeof(Node).IsAssignableFrom(nodeType))
                return;

            List<Node> operators = new List<Node>(windowInfo.selected);
            operators = operators.OrderBy(node => node.priority).ToList();

            windowInfo.selected.Clear();

            windowInfo.treeDirty = true;
            Undo.IncrementCurrentGroup();
            int groupIndex = Undo.GetCurrentGroup();

            for (int i = 0; i < (asChild ? operators.Count : 1); i++)
            {
                if (asChild && !operators[i].canHaveChildren) continue;

                Node node = (Node)ScriptableObject.CreateInstance(nodeType);
                node.hideFlags = HideFlags.HideAndDontSave;
                List<UnityEngine.Object> toRecord = new List<UnityEngine.Object> { node };
                if (node.parent != null) toRecord.Add(node.parent);
                toRecord.AddRange(node.children);

                if (asChild)
                    toRecord.AddRange(operators);

                //Add the node
                Undo.RecordObjects(toRecord.ToArray(), "Add Node");
                //So order changes do not cause errors
                Undo.RegisterCompleteObjectUndo(target, "Add Node");
                node.WorkingNode();
                target.nodes.Add(node);
                node.graph = target;

                Vector2 size = GetArea(node, false);

                if (!asChild)
                {
                    node.position = position - size / 2f;
                }
                else
                {
                    if (operators[i].children.Count > 0)
                    {
                        Node farthest = operators[i].children[operators[i].children.Count - 1];
                        Vector2 farthestSize = GetArea(farthest, false);

                        node.position = new Vector2(farthest.position.x + farthestSize.x + 50f, farthest.position.y);
                    }
                    else
                    {
                        Vector2 parentSize = GetArea(operators[i], false);

                        node.position = new Vector2(operators[i].position.x + parentSize.x / 2f - size.x / 2f, operators[i].position.y + parentSize.y + 150f);
                    }
                }

                if (asChild)
                    AddConnection(operators[i], node);

                Select(node, true);

                Undo.CollapseUndoOperations(groupIndex);

                UpdateSelectors();
            }

            TraverseTree();
            GetViewRect(100f, true);
        }
        private void AddDecorator(Type t)
        {
            if (!typeof(Decorator).IsAssignableFrom(t))
                return;

            windowInfo.treeDirty = true;
            Undo.IncrementCurrentGroup();
            int groupIndex = Undo.GetCurrentGroup();

            foreach (Node node in windowInfo.selected)
            {
                if (node.GetType().Equals(typeof(Root))) return;

                Decorator d = (Decorator)ScriptableObject.CreateInstance(t);
                d.hideFlags = HideFlags.HideAndDontSave;

                Undo.RegisterCompleteObjectUndo(node, "Add Decorator");
                Undo.RecordObject(d, "Add Decorator");

                node.decorators.Add(d);
                d.node = node;

                Vector2 prevArea = GetArea(node, false);
                Vector2 area = GetArea(node, true);

                node.position -= new Vector2(area.x - prevArea.x, area.y - prevArea.y) / 2f;
            }

            Undo.CollapseUndoOperations(groupIndex);

            UpdateSelectors();
            GetViewRect(100f, true);
        }
        private void MoveDecorator(Decorator decorator, Node node)
        {
            if (decorator == null || node == null || node.GetType().Equals(typeof(Root))) return;

            Undo.IncrementCurrentGroup();
            int groupIndex = Undo.GetCurrentGroup();

            Undo.RegisterCompleteObjectUndo(decorator.node, "Move Decorator");
            Undo.RecordObject(decorator, "Move Decorator");
            Undo.RecordObject(node, "Move Decorator");

            Node decoratorNode = decorator.node;

            node.decorators.Add(decorator);
            decoratorNode.decorators.Remove(decorator);

            decorator.node = node;

            GetArea(node, true);
            GetArea(decoratorNode, true);

            SceneView.RepaintAll();

            Undo.CollapseUndoOperations(groupIndex);
        }
        private void MoveDecoratorInNode(Decorator d, bool up)
        {
            if (d == null) return;

            Undo.IncrementCurrentGroup();
            int groupIndex = Undo.GetCurrentGroup();

            string name = String.Format("Move Decorator {0}", up ? "Up" : "Down");

            Undo.RegisterCompleteObjectUndo(d.node, name);
            Undo.RecordObject(d, name);

            int decoratorIndex = d.node.decorators.IndexOf(d);
            d.node.decorators.Swap(decoratorIndex, up ? decoratorIndex - 1 : decoratorIndex + 1);
        }
        private void MoveDecoratorInNode(Decorator d, int index)
        {
            if (d == null || index < 0 || index > d.node.decorators.Count) return;
            Debug.Log("Continuted");

            Undo.IncrementCurrentGroup();
            int groupIndex = Undo.GetCurrentGroup();

            string name = String.Format("Move Decorator To Index {0}", index);

            Undo.RegisterCompleteObjectUndo(d.node, name);
            Undo.RecordObject(d, name);

            int decoratorIndex = d.node.decorators.IndexOf(d);
            d.node.decorators.Move(d, index);
        }
        private void AddConnection(Node n1, Node n2)
        {
            windowInfo.treeDirty = true;
            Undo.RecordObjects(new UnityEngine.Object[] { n1, n2 }, "Add Connection");
            if (!n1.children.Contains(n2))
            {
                n1.children.Add(n2);
            }
            n2.parent = n1;
            orphanNode = null;
        }

        private void RemoveConnection(Node n1, Node n2)
        {
            windowInfo.treeDirty = true;
            Undo.RecordObjects(new UnityEngine.Object[] { n1, n2 }, "Remove Connection");
            n1.children.Remove(n2);
            n2.parent = null;
            TraverseTree();
            GetViewRect(100f, true);
        }

        public void Duplicate(List<Node> original, List<Node> tree, bool select, bool clearSelected = true)
        {
            windowInfo.treeDirty = true;
            //identify "root" nodes
            List<Node> roots = original.FindAll(node =>
            {
                if (!original.Contains(node.parent))
                {
                    return true;
                }

                return node.parent == null ? true : false;
            }
            );

            if (clearSelected)
                windowInfo.selected.Clear();

            //and recursively duplicate
            for (int i = 0; i < roots.Count; i++)
            {
                Node node = roots[i];

                DuplicateRecursive(original, tree, node, null, select);
            }
        }
        private Node DuplicateRecursive(List<Node> toDuplicate, List<Node> tree, Node original, Node parent, bool select)
        {
            toDuplicate.ForEach(x => Debug.Log(x.Name));
            Node node = Instantiate(original);
            node.parent = parent;
            node.uID = Guid.NewGuid().ToString("N");
            node.hideFlags = HideFlags.HideAndDontSave;

            for (int i = 0; i < node.decorators.Count; i++)
            {
                node.decorators[i] = Instantiate(node.decorators[i]);
                node.decorators[i].uID = Guid.NewGuid().ToString("N");
                node.decorators[i].node = node;
                node.hideFlags = HideFlags.HideAndDontSave;
            }

            node.children.RemoveAll(n => !toDuplicate.Contains(n));
            node.children = node.children.Select(n =>
            {
                Debug.Log(n.Name);
                return DuplicateRecursive(toDuplicate, tree, n, node, select);
            }).ToList();

            tree.Add(node);

            if (select) Select(node, true);
            return node;
        }
        private Node GetSiblingNode(Node node, bool left)
        {
            if (node == null || node.parent == null)
                return null;

            Node parent = node.parent;
            int thisIndex = node.parent.children.IndexOf(node);

            if (left && thisIndex == 0)
                return null;
            else if (!left && thisIndex == parent.children.Count - 1)
                return null;

            return parent.children[left ? thisIndex - 1 : thisIndex + 1];
        }
        internal GenericMenu GenerateContextMenu()
        {
            GenericMenu g = new GenericMenu();

            switch (windowInfo.hoveredType)
            {
                case Window.Hovering.Node:
                    g = GenerateNodeContextMenu();
                    break;
                case Window.Hovering.InConnection:
                    g.AddItem("Break Connection", false, () => RemoveConnection(windowInfo.hoveredNode.parent, windowInfo.hoveredNode), editingPaused);
                    break;
                case Window.Hovering.OutConnection:
                    g.AddItem("Break Connection", false, () => { }, editingPaused);
                    break;
                case Window.Hovering.Decorator:
                    g.AddItem(GenerateMenuItem("Main Menu/Edit/Cut"), false, () => { }, editingPaused);
                    g.AddItem(GenerateMenuItem("Main Menu/Edit/Copy"), false, () => { }, editingPaused);
                    g.AddItem(GenerateMenuItem("Main Menu/Edit/Paste"), false, () => { }, editingPaused || copyBuffer.Count == 0);
                    g.AddItem(GenerateMenuItem("Main Menu/Edit/Delete"), false, () => Delete(windowInfo.selectedDecorator), editingPaused);
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
                    g.AddItem(GenerateMenuItem("Schema/Zoom In"), false, () => ZoomInCommand(), false);
                    g.AddItem(GenerateMenuItem("Schema/Zoom Out"), false, () => ZoomOutCommand(), false);
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
            g.AddItem("Break Connections %b", false, () =>
            {
                foreach (Node node in windowInfo.selected)
                {
                    if (node.parent)
                        RemoveConnection(node.parent, node);

                    List<Node> ch = new List<Node>(node.children);
                    foreach (Node c in ch)
                    {
                        RemoveConnection(node, c);
                    }
                }
            }, editingPaused);
            g.AddSeparator("");
            g.AddItem(GenerateMenuItem("Schema/Add Decorator"), false, () => AddDecoratorCommand(), editingPaused);
            g.AddItem(GenerateMenuItem("Schema/Add Child"), false, () => AddChildCommand(), !windowInfo.hoveredNode.canHaveChildren);

            return g;
        }
        internal GenericMenu GenerateAddMenu()
        {
            GenericMenu g = new GenericMenu();

            //Main loop, which has the current type that we are adding to the menu
            foreach (KeyValuePair<Type, List<Type>> kvp in nodeTypes)
            {
                if (kvp.Value.Count == 0 && !kvp.Key.IsAbstract)
                    g.AddItem(new GUIContent(kvp.Key.Name), false, () => AddNode(kvp.Key, Vector2.zero, true));

                foreach (Type nodeType in kvp.Value)
                {
                    //this needs to be done to access overiden properties. Since we can't override constants (such as category),
                    //they need to be readonly properties that we access thorugh an instance

                    Node instance = (Node)ScriptableObject.CreateInstance(nodeType);

                    string category = instance.category;

                    string s = "";

                    if (String.IsNullOrEmpty(category))
                        s = kvp.Key.Name + "/";

                    if (!String.IsNullOrEmpty(category)) s += category;
                    if (s.Length > 0 && s.Substring(s.Length - 1) != "/") s += "/";

                    s += nodeType.Name;

                    g.AddItem(s, false, () => AddNode(nodeType, Vector2.zero, true), editingPaused);

                    DestroyImmediate(instance);
                }
            }

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
                nodeCount = target.nodes.Count;

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
        [Shortcut("Schema/Zoom In", KeyCode.Equals, ShortcutModifiers.Shift)]
        private static void ZoomInCommand()
        {
            if (instance == null) return;

            instance.windowInfo.zoom -= 3 * GUIData.zoomSpeed;
        }
        [Shortcut("Schema/Zoom Out", KeyCode.Minus, ShortcutModifiers.Shift)]
        private static void ZoomOutCommand()
        {
            if (instance == null) return;

            instance.windowInfo.zoom += 3 * GUIData.zoomSpeed;
        }
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
            if (instance == null || instance.windowInfo.selected.Count == 0 || !instance.windowInfo.selected[0].canHaveChildren) return;

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
            private static Color GetColor(string key, Color defaultValue)
            {
                float r = EditorPrefs.GetFloat(key + "_r", defaultValue.r);
                float g = EditorPrefs.GetFloat(key + "_g", defaultValue.g);
                float b = EditorPrefs.GetFloat(key + "_b", defaultValue.b);
                float a = EditorPrefs.GetFloat(key + "_a", defaultValue.a);

                return new Color(r, g, b, a);
            }
            private static void SetColor(string key, Color value)
            {
                EditorPrefs.SetFloat(key + "_r", value.r);
                EditorPrefs.SetFloat(key + "_g", value.g);
                EditorPrefs.SetFloat(key + "_b", value.b);
                EditorPrefs.SetFloat(key + "_a", value.a);
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