using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using Schema;
using Schema.Utilities;
using Schema.Editor.Utilities;
using System.Linq;

namespace SchemaEditor
{
    public partial class NodeEditor
    {
        private Matrix4x4 prevMatrix;
        private bool drawBox;
        private float tabHeight => isDocked() ? 19.0f : 21.0f;

        private Func<bool> isDocked => isDockedFunc ??= this.GetIsDockedDelegate();
        private Func<bool> isDockedFunc;
        private UnityEditor.Editor editor;
        private UnityEditor.Editor blackboardEditor;
        private UnityEditor.Editor defaultNodeEditor;
        private UnityEditor.Editor defaultDecoratorEditor;
        private List<Type> distinctTypes;
        private SchemaAgent activeAgent;
        private bool editingPaused;
        private bool needsPan;
        //Focus of the search box needs to be delayed by one frame, or else the keyboard shortcut triggering the search box will type in the box
        private bool searchWantsFocus;
        private bool shouldFocusSearch;
        private Rect window;
        private void OnGUI()
        {
            if (target != null)
            {
                ProcessEvents(Event.current);

                if (Event.current.type == EventType.Layout) LayoutGUI();

                if (windowInfo.inspectorToggled)
                    window = new Rect(0f, 0f, position.width - windowInfo.inspectorWidth - GUIData.sidebarPadding * 2, position.height);
                else
                    window = new Rect(0f, 0f, position.width, position.height);
                windowInfo.selected.RemoveAll(node => node == null);

                for (int i = 0; i < windowInfo.changedNodes.Count; i++)
                    GetArea(windowInfo.changedNodes.Dequeue(), true);

                DrawGrid(window, windowInfo.zoom, windowInfo.pan);
                DrawConnections();
                DrawNodes();
                DrawWindow();
                DrawMinimap();
                DrawToolbar();
                DrawInspector();

                Blackboard.instance = target.blackboard;

                Vector2 size;

                if (windowInfo.selectedDecorator)
                {
                    //Draw legend here

                    size = EditorStyles.label.CalcSize(new GUIContent("Lower Priority Nodes"));
                    GUI.Label(new Rect(position.width - size.x - (windowInfo.inspectorWidth + GUIData.sidebarPadding * 2) - 32f, position.height - size.y - 8f, size.x, size.y),
                     new GUIContent("Lower Priority Nodes"), EditorStyles.label);

                    GUI.color = Styles.lowerPriorityColor;
                    GUI.Label(new Rect(position.width - (windowInfo.inspectorWidth + GUIData.sidebarPadding * 2) - 24f, position.height - size.y / 2f - 16f, 16f, 16f),
                        "",
                        Styles.styles.decorator);
                    GUI.color = Color.white;

                    float lastHeight = size.y + 16f;
                    size = EditorStyles.label.CalcSize(new GUIContent("Self Nodes"));

                    //Draw legend here
                    GUI.Label(new Rect(position.width - size.x - (windowInfo.inspectorWidth + GUIData.sidebarPadding * 2) - 32f, position.height - lastHeight - size.y - 8f, size.x, size.y),
                     new GUIContent("Self Nodes"), EditorStyles.label);

                    GUI.color = Styles.selfColor;
                    GUI.Label(new Rect(position.width - (windowInfo.inspectorWidth + GUIData.sidebarPadding * 2) - 24f, position.height - lastHeight - size.y / 2f - 16f, 16f, 16f),
                        "",
                        Styles.styles.decorator);
                    GUI.color = Color.white;
                }

                if (activeAgent != null)
                {
                    string content = $"Viewing GameObject {activeAgent.gameObject.name}. t={Time.time}";
                    size = EditorStyles.boldLabel.CalcSize(new GUIContent(content));
                    GUI.Label(new Rect(8f, 32f, size.x, size.y), content, EditorStyles.boldLabel);
                }

                if (NodeEditorPrefs.enableDebugView && windowInfo != null)
                {
                    string content = @$"hoveredNode: {windowInfo.hoveredNode?.name}
hoveredDecorator: {windowInfo.hoveredDecorator?.name}
hoveredType: {windowInfo.hoveredType}
hoveredConnection: {windowInfo.hoveredConnection}

";

                    if (windowInfo.selected.Count == 1)
                    {
                        content += @$"Position: {windowInfo.selected[0].position.ToString()}
Area: {GetAreaWithPadding(windowInfo.selected[0], false).ToString()}
Parent: {windowInfo.selected[0].parent?.name}
Children: {String.Join(", ", windowInfo.selected[0]?.children.Select(node => node.name))}";
                    }

                    size = EditorStyles.miniLabel.CalcSize(new GUIContent(content));
                    GUI.Label(new Rect(8f, 32f, size.x, size.y), content, EditorStyles.miniLabel);
                }

                editingPaused = Application.isPlaying;

                if (editingPaused)
                {
                    string content = "In edit mode, only modification of Node and Decorator properties is allowed.";
                    size = EditorStyles.boldLabel.CalcSize(new GUIContent(content));
                    GUI.Label(new Rect(8f, position.height - size.y - 8f, size.x, size.y), content, EditorStyles.boldLabel);
                }

                if (windowInfo.isPanning) SetCursor(MouseCursor.Pan);
                else if (windowInfo.resizingInspector || windowInfo.hoverDivider) SetCursor(MouseCursor.ResizeHorizontal);

                if (needsPan)
                {
                    if (windowInfo.isPanning)
                        needsPan = false;

                    windowInfo.pan = Vector2.Lerp(windowInfo.pan, windowInfo.nextPan, Mathf.SmoothStep(0f, 1f, (Time.realtimeSinceStartup - windowInfo.recordedTime) / windowInfo.panDuration));

                    Vector2 diff = windowInfo.pan - windowInfo.nextPan;

                    if (Mathf.Abs(diff.x) < 0.01f && Mathf.Abs(diff.y) < 0.01f)
                        needsPan = false;
                }
            }
            else
            {
                DrawGrid(position, 1f, Vector2.zero);

                const float width = 450f;
                const float height = 500f;

                BeginWindows();
                GUILayout.Window(1, new Rect(position.width / 2f - width / 2f, position.height / 2f - height / 2f, width, height), DoSplashWindow, "", Styles.styles.addNodeWindow);
                EndWindows();
            }

            if (windowInfo != null)
                windowInfo.timeLastFrame = Time.realtimeSinceStartup;
            Repaint();
        }

        void LayoutGUI()
        {
            List<UnityEngine.Object> targets = new List<UnityEngine.Object>();
            List<BlackboardEntry> duplicateEntries = new List<BlackboardEntry>();

            for (int i = 0; i < target.blackboard.entries.Length; i++)
            {
                for (int j = 0; j < target.blackboard.entries.Length; j++)
                {
                    if (j == i || duplicateEntries.Contains(target.blackboard.entries[j]))
                        continue;

                    if (target.blackboard.entries[i] == target.blackboard.entries[j])
                        duplicateEntries.Add(target.blackboard.entries[j]);
                }
            }

            foreach (BlackboardEntry e in duplicateEntries)
            {
                target.blackboard.entries[Array.IndexOf(target.blackboard.entries, e)] = ScriptableObject.Instantiate(e);
            }

            if (blackboardEditor && ((BlackboardEditor)blackboardEditor).selectedEntry != null)
            {
                targets.Add(((BlackboardEditor)blackboardEditor).selectedEntry);
            }
            else if (windowInfo.selectedDecorator != null)
            {
                targets.Add(windowInfo.selectedDecorator);

                if (!defaultDecoratorEditor || defaultDecoratorEditor.target == null || defaultDecoratorEditor.target != windowInfo.selectedDecorator)
                    UnityEditor.Editor.CreateCachedEditor(windowInfo.selectedDecorator, typeof(DefaultDecoratorEditor), ref defaultDecoratorEditor);
            }
            else if (windowInfo.selected.Count > 0)
            {
                targets.AddRange(windowInfo.selected);

            }

            distinctTypes = targets.Where(x => x != null).Select(x => x.GetType()).Distinct().ToList();

            if (distinctTypes.Count > 1) return;

            //caches inspector once per OnGUI call so we don't get EventType.Layout and EventType.Repaint related errors
            if (editor == null || editor.targets.Any(obj => obj == null) || !editor.targets.SequenceEqual(targets) && targets.Count > 0 && targets.All(x => x != null))
            {
                UnityEditor.Editor.CreateCachedEditor(targets.ToArray(), null, ref editor);

                if (windowInfo.selectedDecorator == null && distinctTypes.Count == 1 && (defaultNodeEditor == null || defaultNodeEditor.targets.Any(obj => obj == null) || !defaultNodeEditor.targets.SequenceEqual(targets)) && targets.Count > 0 && targets.All(obj => obj != null))
                    UnityEditor.Editor.CreateCachedEditor(targets.ToArray(), typeof(DefaultNodeEditor), ref defaultNodeEditor);
            }

            // ReSharper disable once Unity.NoNullPropagation
            SchemaAgent agent = Selection.activeGameObject?.GetComponent<SchemaAgent>();

            if (Application.isPlaying)
            {
                if (!activeAgent || (agent != null && agent != activeAgent && agent.target == target))
                {
                    activeAgent = agent;
                    editingPaused = true;
                }
            }
            else
            {
                activeAgent = null;
            }

            if (agent)
            {
                if (windowInfo.selectedDecorator != null)
                    agent.editorTarget = windowInfo.selectedDecorator.node;
                else if (editor != null && editor.targets.All(target => typeof(Node).IsAssignableFrom(target.GetType())))
                    agent.editorTarget = (Node)editor.targets[0];
            }

            switch (searchWantsFocus)
            {
                case true when shouldFocusSearch:
                    EditorGUI.FocusTextInControl("SearchTextField");
                    searchWantsFocus = false;
                    shouldFocusSearch = false;
                    break;
                case true when !shouldFocusSearch:
                    shouldFocusSearch = true;
                    break;
            }
        }

        ///<summary>
        ///Draws links between nodes and their children and parents
        ///</summary>
        private void DrawConnections()
        {
            BeginZoomed(window, windowInfo.zoom, tabHeight);

            Vector2 mousePos = Event.current.mousePosition;

            foreach (Node node in target.nodes)
            {
                Vector2 nodeSize = GetArea(node, false);

                foreach (Node child in node.children)
                {
                    if (orphanNode == child) continue;

                    Vector2 childSize = GetArea(child, false);

                    Vector2 from = GridToWindowPositionNoClipped(new Vector2(
                        node.position.x + (nodeSize.x + GUIData.nodePadding * 2) / 2f,
                        node.position.y + nodeSize.y + GUIData.nodePadding * 2 + 16f));
                    Vector2 to = GridToWindowPositionNoClipped(new Vector2(
                        child.position.x + (childSize.x + GUIData.nodePadding * 2) / 2f,
                        child.position.y - 16f));

                    Vector2 p0 = from;
                    Vector2 p1 = from + Vector2.up * 50f;
                    Vector2 p2 = to - Vector2.up * 50f;
                    Vector2 p3 = new Vector2(to.x, to.y - 8f * windowInfo.zoom);

                    CurveUtility.Bezier bezier = new CurveUtility.Bezier(p0, p1, p2, p3);

                    Node active = activeAgent?.GetRunningNode();

                    bool isActiveConnection = (EditorApplication.isPlaying
                        || EditorApplication.isPaused)
                        && active != null
                        && IsSubTreeOf(node, active)
                        && (child == active || IsSubTreeOf(child, active));

                    bool intersect = false;

                    Node selected = null;

                    if (windowInfo.selected.Count == 1)
                        selected = windowInfo.selected[0];

                    if (selected != null && selected.canHaveParent && selected.CanHaveChildren() && selected.parent != node && selected != node)
                        intersect = bezier.Intersect(
                            new Rect(
                                GridToWindowPositionNoClipped(selected.position),
                                GetAreaWithPadding(selected, false)
                            )
                        );

                    if (intersect && windowInfo.hoveredConnection != child && windowInfo.shouldCheckConnectionHover)
                        windowInfo.hoveredConnection = child;
                    else if (!intersect && windowInfo.hoveredConnection == child && windowInfo.shouldCheckConnectionHover)
                        windowInfo.hoveredConnection = null;

                    Handles.DrawBezier(
                        p0,
                        p3,
                        p1,
                        p2,
                        isActiveConnection ? NodeEditorPrefs.highlightColor : (windowInfo.hoveredConnection == child ? Color.white : new Color(0.5f, 0.5f, 0.5f, 1f)),
                        null,
                        3f * windowInfo.zoom
                    );

                    if (isActiveConnection)
                    {
                        const float fac = 1.5f;
                        const int points = 4;

                        for (int i = 0; i < points; i++)
                        {
                            float t = (float)((EditorApplication.timeSinceStartup % fac) / fac);
                            t += 1f / (float)points * i;
                            t = t > 1 ? t % 1 : t;
                            Vector2 p = bezier.Position(t);

                            GUI.color = NodeEditorPrefs.highlightColor;
                            GUI.DrawTexture(new Rect(p.x - 4f * windowInfo.zoom, p.y - 4f * windowInfo.zoom, 8f * windowInfo.zoom, 8f * windowInfo.zoom), Styles.circle);
                        }
                    }

                    Rect r = new Rect(to.x - 4f * windowInfo.zoom, to.y - 8f * windowInfo.zoom, 8f * windowInfo.zoom, 8f * windowInfo.zoom);

                    GUI.color = isActiveConnection ? NodeEditorPrefs.highlightColor : new Color(.5f, .5f, .5f, 1f);

                    GUI.DrawTexture(r, Styles.arrow);

                    GUI.color = Color.white;
                }
            }

            if (requestingConnection != null)
            {
                Node node = requestingConnection;
                Vector2 nodeSize = GetArea(node, false);

                Vector2 from = GridToWindowPositionNoClipped(new Vector2(
                    node.position.x + (nodeSize.x + GUIData.nodePadding * 2) / 2f,
                    node.position.y + nodeSize.y + GUIData.nodePadding * 2 + 16f));
                Vector2 to = new Vector2(mousePos.x, mousePos.y - 8f * windowInfo.zoom);

                Handles.DrawBezier(
                    from,
                    to,
                    from + Vector2.up * 50f,
                    to - Vector2.up * 50f,
                    Color.white,
                    null,
                    3f * windowInfo.zoom
                );

                Rect r = new Rect(to.x - 4f * windowInfo.zoom, to.y, 8f * windowInfo.zoom, 8f * windowInfo.zoom);

                GUI.color = new Color(.85f, .85f, .85f, 1f);

                GUI.DrawTexture(r, Styles.arrow);
            }
            EndZoomed();
        }
        private void DrawNodes()
        {
            Event current = Event.current;

            //these will be overriden later if the nodes, decorators, or inspector contains the mouse position
            if (window.Contains(current.mousePosition) && IsNotLayoutEvent(current))
                windowInfo.hoveredType = Window.Hovering.Window;
            else if (IsNotLayoutEvent(current))
                windowInfo.hoveredType = Window.Hovering.None;


            BeginZoomed(window, windowInfo.zoom, tabHeight);

            List<Node> nodes = target.nodes.ToList();

            if (nodes != null)
            {
                Vector2 boxStartPos = GridToWindowPositionNoClipped(windowInfo.mouseDownPos);
                Vector2 boxSize = current.mousePosition - boxStartPos;
                if (boxSize.x < 0) { boxStartPos.x += boxSize.x; boxSize.x = Mathf.Abs(boxSize.x); }
                if (boxSize.y < 0) { boxStartPos.y += boxSize.y; boxSize.y = Mathf.Abs(boxSize.y); }
                Rect selectionBox = new Rect(boxStartPos, boxSize);

                List<Node> selectionQueue = new List<Node>();

                bool didHoverDecoratorThisFrame = false;

                if (activeAgent != null)
                {
                    foreach (Node node in activeAgent.GetCalledNodes())
                        NodeTicked(node);

                    windowInfo.nodeStatus = activeAgent.GetNodeStatus();
                }

                for (int i = nodes.Count - 1; i >= 0; i--)
                {
                    Node node = nodes[i];
                    Vector2 positionNoClipped = GridToWindowPositionNoClipped(node.position);
                    Vector2 size = GetArea(node, false);
                    Vector2 sizeWithPadding = GetAreaWithPadding(node, false);

                    Rect contained = new Rect(positionNoClipped.x + GUIData.nodePadding, positionNoClipped.y + GUIData.nodePadding, size.x, size.y);
                    Rect rect = new Rect(positionNoClipped, new Vector2(sizeWithPadding.x, sizeWithPadding.y));

                    GUI.color = Styles.windowBackground;

                    GUI.Box(rect, "", Styles.styles.node);

                    if (windowInfo.selected.Contains(node))
                        GUI.color = NodeEditorPrefs.selectionColor;
                    else if (windowInfo.alpha.ContainsKey(node.uID))
                        GUI.color = Color.Lerp(new Color32(80, 80, 80, 255), NodeEditorPrefs.highlightColor, windowInfo.alpha[node.uID]);
                    else if (windowInfo.selectedDecorator &&
                        node.priority > 0 &&
                        windowInfo.selectedDecorator.node.priority > 0 &&
                        (windowInfo.selectedDecorator.abortsType == Decorator.ObserverAborts.Self ||
                        windowInfo.selectedDecorator.abortsType == Decorator.ObserverAborts.Both
                        ) &&
                        IsSubTreeOf(windowInfo.selectedDecorator.node, node))
                        GUI.color = Styles.selfColor;
                    else if (windowInfo.selectedDecorator &&
                        node.priority > 0 &&
                        windowInfo.selectedDecorator.node.priority > 0 &&
                        (windowInfo.selectedDecorator.abortsType == Decorator.ObserverAborts.LowerPriority ||
                        windowInfo.selectedDecorator.abortsType == Decorator.ObserverAborts.Both
                        ) &&
                        IsLowerPriority(windowInfo.selectedDecorator.node, node) &&
                        windowInfo.selectedDecorator.node.priority < node.priority)
                        GUI.color = Styles.lowerPriorityColor;
                    else if (EditorGUIUtility.isProSkin)
                        GUI.color = new Color32(80, 80, 80, 255);

                    GUI.Box(rect, "", Styles.styles.nodeSelected);

                    GUI.color = Color.white;

                    bool blocked = false;

                    if (node.priority > 0)
                    {
                        Handles.color = Styles.windowAccent;
                        Handles.DrawAAConvexPolygon(HelperMethods.Circle(new Vector2(rect.x, rect.center.y), 15f, 18));
                        Handles.color = Color.white;

                        GUI.Label(new Rect(rect.x - 15f, rect.center.y - 15f, 30f, 30f), node.priority.ToString(), Styles.styles.title);
                    }

                    GUIContent error = GetErrors(node);

                    if (error != GUIContent.none)
                    {
                        float iconWidth = error.image.width;
                        float iconHeight = error.image.height;
                        GUI.Label(new Rect(rect.x + rect.width - iconWidth / 2f, rect.y + rect.height - iconHeight / 2f, iconWidth, iconHeight), error, GUIStyle.none);
                    }

                    if (NodeEditorPrefs.enableStatusIndicators && node.enableStatusIndicator && windowInfo.nodeStatus != null && Application.isPlaying && windowInfo.nodeStatus.ContainsKey(node.uID))
                    {
                        float iconSize = 32f;

                        bool? nodeStatus = windowInfo.nodeStatus[node.uID];
                        if (nodeStatus == true)
                            GUI.color = NodeEditorPrefs.successColor;
                        else if (nodeStatus == false)
                            GUI.color = NodeEditorPrefs.failureColor;

                        GUI.Label(new Rect(rect.x + rect.width - iconSize / 2f, rect.y - iconSize / 2f, iconSize, iconSize),
                            "",
                            Styles.styles.decorator);
                    }

                    GUI.color = Color.white;

                    GUILayout.BeginArea(contained);
                    GUILayout.BeginVertical();

                    List<float> positions = new List<float>();

                    for (int j = 0; j < node.decorators.Length; j++)
                    {
                        Decorator d = node.decorators[j];

                        if (d == null)
                            continue;

                        bool isSelected = windowInfo.selectedDecorator == d;

                        GUI.color = isSelected ? new Color(.6f, .6f, .1f, 1f) : new Color(.1f, .1f, .4f, 1f);

                        GUILayout.BeginVertical(Styles.styles.decorator);

                        GUI.color = Color.white;

                        GUILayout.Label(d.name, Styles.styles.nodeLabel, GUILayout.Height(GUIData.labelHeight), GUILayout.ExpandWidth(true));

                        GUILayout.Label(d.GetInfoContent(), Styles.styles.nodeText);

                        GUILayout.Space(GUIData.spacing / 2f);

                        GUILayout.EndVertical();

                        Rect last = GUILayoutUtility.GetLastRect();

                        positions.Add(last.position.y - (GUIData.spacing / 2f));

                        if (last.Contains(current.mousePosition))
                        {
                            windowInfo.hoveredDecorator = d;
                            windowInfo.hoveredType = Window.Hovering.Decorator;
                            didHoverDecoratorThisFrame = true;
                            blocked = true;
                        }
                        else if (!didHoverDecoratorThisFrame)
                        {
                            windowInfo.hoveredDecorator = null;
                        }

                        GUILayout.Space(GUIData.spacing);

                        if (j == node.decorators.Length - 1)
                        {
                            //last item, add extra position to snap to
                            positions.Add(last.position.y + last.size.y + (GUIData.spacing / 2f));
                        }
                    }

                    Rect toDraw = new Rect();
                    bool draw = node.decorators.Length > 0 && (windowInfo.hoveredNode == node || windowInfo.hoveredType == Window.Hovering.Window) && IsNotLayoutEvent(Event.current);

                    if (draw)
                    {
                        float closest = positions[0];

                        for (int j = 1; j < positions.Count; j++)
                        {
                            float pos = positions[j];

                            closest = Mathf.Abs(current.mousePosition.y - pos) < Mathf.Abs(current.mousePosition.y - closest) ? pos : closest;
                        }

                        if (node.decorators.Contains(windowInfo.selectedDecorator))
                            windowInfo.hoveredDecoratorIndex = positions.IndexOf(closest);

                        toDraw = new Rect(new Vector2(0f, closest - GUIData.spacing / 8f), new Vector2(size.x, GUIData.spacing / 4f));
                    }

                    GUI.color = Styles.windowAccent;

                    float contentHeight = Mathf.Max((node.icon == null ? 0 : node.icon.height + 10f) + GUIData.labelHeight, GUIData.minContentHeight);

                    GUILayout.BeginVertical(Styles.styles.decorator, GUILayout.Height(contentHeight));

                    GUI.color = Color.white;

                    if (node.icon != null)
                    {
                        GUILayout.Space(5f);
                        GUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                        GUILayout.Label(node.icon);
                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();
                        GUILayout.Space(5f);
                    }
                    GUILayout.Label(node.name, Styles.styles.nodeLabel, GUILayout.ExpandHeight(true));

                    GUILayout.EndVertical();

                    GUILayout.EndVertical();

                    GUILayout.EndArea();

                    if (draw && windowInfo.lastClicked == Window.Hovering.Decorator && windowInfo.didDragSinceMouseUp && node.decorators.Contains(windowInfo.selectedDecorator))
                    {
                        toDraw.position += contained.position;

                        EditorGUI.DrawRect(toDraw, new Color32(200, 200, 200, 255));
                    }

                    if (node.canHaveParent)
                    {
                        float width = size.x - GUIData.nodePadding * 2;
                        Rect inConnection = new Rect(positionNoClipped.x + GUIData.nodePadding + size.x / 2f - width / 2f, positionNoClipped.y - 16f, width, 16f);

                        if (inConnection.Contains(current.mousePosition) && IsNotLayoutEvent(current))
                        {
                            windowInfo.hoveredType = Window.Hovering.InConnection;
                            windowInfo.hoveredNode = node;
                        }

                        if (windowInfo.hoveredType == Window.Hovering.InConnection && windowInfo.hoveredNode == node && !drawBox)
                            GUI.color = Color.white;
                        else
                            GUI.color = Styles.windowAccent;

                        GUI.Box(inConnection, "", Styles.styles.decorator);
                    }

                    if (node.maxChildren > 0)
                    {
                        float width = size.x - GUIData.nodePadding * 2;
                        Rect outConnection = new Rect(positionNoClipped.x + GUIData.nodePadding + size.x / 2f - width / 2f, positionNoClipped.y + size.y + GUIData.nodePadding * 2, width, 16f);

                        if (outConnection.Contains(current.mousePosition) && IsNotLayoutEvent(current))
                        {
                            windowInfo.hoveredType = Window.Hovering.OutConnection;
                            windowInfo.hoveredNode = node;
                        }

                        if (windowInfo.hoveredType == Window.Hovering.OutConnection && windowInfo.hoveredNode == node && !drawBox)
                            GUI.color = Color.white;
                        else
                            GUI.color = Styles.windowAccent;

                        GUI.Box(outConnection, "", Styles.styles.decorator);
                    }

                    if (rect.Contains(current.mousePosition) && IsNotLayoutEvent(current))
                    {
                        windowInfo.hoveredNode = node;
                        if (!blocked)
                        {
                            windowInfo.hoveredType = Window.Hovering.Node;
                        }
                    }

                    if (rect.Overlaps(selectionBox) && IsNotLayoutEvent(current) && drawBox && windowInfo.didDragSinceMouseUp)
                    {
                        selectionQueue.Add(node);
                    }
                    else if (drawBox && windowInfo.didDragSinceMouseUp && IsNotLayoutEvent(current))
                    {
                        if (windowInfo.selected.Contains(node))
                        {
                            windowInfo.selected.Remove(node);
                        }
                    }
                }

                List<string> keys = windowInfo.alpha.Keys.ToList();

                foreach (string node in keys)
                {
                    windowInfo.alpha[node] -= 2f * Mathf.Clamp(windowInfo.deltaTime, 0f, float.MaxValue);

                    if (windowInfo.alpha[node] <= 0f)
                    {
                        windowInfo.alpha.Remove(node);
                    }
                }

                for (int i = selectionQueue.Count - 1; i >= 0; i--)
                {
                    if (!windowInfo.selected.Contains(selectionQueue[i]))
                    {
                        Select(selectionQueue[i], true);
                    }
                }
            }

            GUI.color = Color.white;

            EndZoomed();
        }

        private void NodeTicked(Node node)
        {
            windowInfo.alpha[node.uID] = 1f;
        }
        ///<summary>
        ///This function calculates the area of a node, based on its text and calculates decorators (based on contents themselves, does not factor in padding)
        ///</summary>
        internal static Vector2 GetArea(Node node, bool recalculate)
        {
            if (node == null) return Vector2.zero;

            //try to get from dictionary
            if (GUIData.sizes.ContainsKey(node) && !recalculate)
            {
                return GUIData.sizes[node];
            }

            //get size of contents
            float height = Mathf.Max((node.icon == null ? 0 : node.icon.height + 10f) + GUIData.labelHeight, GUIData.minContentHeight);
            float width = Mathf.Max(Styles.styles.nodeLabel.CalcSize(new GUIContent(node.name)).x, node.icon == null ? 0 : node.icon.width);

            foreach (Decorator decorator in node.decorators)
            {
                Debug.Log(decorator);

                if (decorator == null)
                    continue;

                float decoratorLabelWidth = Styles.styles.nodeLabel.CalcSize(new GUIContent(decorator.name)).x;

                width = Mathf.Max(width, decoratorLabelWidth);
                height += GUIData.labelHeight;

                Vector2 infoSize = Styles.styles.nodeText.CalcSize(decorator.GetInfoContent());

                height += infoSize.y;
                width = Mathf.Max(width, infoSize.x);

                //The 4 is accounting for the area that GUILayout.BeginVertical adds when applying a background for decorators. Not sure why this happens.
                height += GUIData.spacing * 1.5f + 4;
            }

            Vector2 final = new Vector2(width + 40f, height);

            if (recalculate)
                GUIData.sizes[node] = final;
            else
                GUIData.sizes.Add(node, final);

            return final;
        }
        internal static Vector2 GetAreaWithPadding(Node node, bool recalculate)
        {
            return GetArea(node, recalculate) + Vector2.one * GUIData.nodePadding * 2;
        }
        private static bool IsNotLayoutEvent(Event e)
        {
            return e.type != EventType.Layout;
        }
        private void DrawWindow()
        {
            //Draw selection box
            if (drawBox)
            {
                Vector2 curPos = WindowToGridPosition(Event.current.mousePosition);
                Vector2 size = curPos - windowInfo.mouseDownPos;
                Rect r = new Rect(windowInfo.mouseDownPos, size);
                r.position = GridToWindowPosition(r.position);
                r.size /= windowInfo.zoom;

                Handles.DrawSolidRectangleWithOutline(r, new Color(0, 0, 0, 0.1f), new Color(1, 1, 1, 0.6f));
            }

            if (windowInfo.searchIsShown)
                windowInfo.searchRect = RenderSearch(windowInfo.searchRect);

            if (windowInfo.selected.Count == 1 || windowInfo.selectedDecorator != null)
            {
                string stringContent = windowInfo.selectedDecorator != null ? windowInfo.selectedDecorator.description : windowInfo.selected[0].description;

                if (!String.IsNullOrEmpty(stringContent))
                {
                    GUIStyle s = new GUIStyle(EditorStyles.boldLabel);
                    s.fixedWidth = Mathf.Min(500f, window.width - GUIData.nodePadding * 2f);
                    s.wordWrap = true;

                    GUIContent content = new GUIContent(stringContent);
                    float height = s.CalcHeight(content, s.fixedWidth);
                    Vector2 size = new Vector2(s.fixedWidth, height);
                    GUI.Label(new Rect(new Vector2(GUIData.nodePadding, position.height - GUIData.nodePadding - height), size), content, s);
                }
            }
        }
        private void DrawMinimap()
        {
            if (!windowInfo.drawMinimap)
                return;

            float minimapPadding = 100f;

            Rect viewRect = GetViewRect(minimapPadding, false);

            float minimapHeight = viewRect.height / viewRect.width * NodeEditorPrefs.minimapWidth;
            float viewWidth = minimapHeight >= NodeEditorPrefs.maxMinimapHeight ? NodeEditorPrefs.minimapWidth / minimapHeight * NodeEditorPrefs.maxMinimapHeight : NodeEditorPrefs.minimapWidth;
            minimapHeight = Mathf.Clamp(minimapHeight, 0f, NodeEditorPrefs.maxMinimapHeight);

            Rect boxPos = Rect.zero;

            switch (NodeEditorPrefs.minimapPosition)
            {
                case 0:
                    boxPos = new Rect(10f, position.height - minimapHeight - 10f, NodeEditorPrefs.minimapWidth, minimapHeight);
                    break;
                case 1:
                    boxPos = new Rect(10f, EditorStyles.toolbar.fixedHeight + 10f, NodeEditorPrefs.minimapWidth, minimapHeight);
                    break;
                case 2:
                    boxPos = new Rect(window.width - 10f - NodeEditorPrefs.minimapWidth, position.height - minimapHeight - 10f, NodeEditorPrefs.minimapWidth, minimapHeight);
                    break;
                case 3:
                    boxPos = new Rect(window.width - 10f - NodeEditorPrefs.minimapWidth, EditorStyles.toolbar.fixedHeight + 10f, NodeEditorPrefs.minimapWidth, minimapHeight);
                    break;
            }

            Rect gridViewRect = WindowToGridRect(window);
            gridViewRect.position = GridToMinimapPosition(gridViewRect.position, viewWidth, minimapPadding);
            gridViewRect.position = new Vector2(gridViewRect.position.x + boxPos.width / 2f - viewWidth / 2f, gridViewRect.position.y);
            gridViewRect.size = gridViewRect.size / viewRect.width * viewWidth;

            Handles.DrawSolidRectangleWithOutline(boxPos, new Color(0.1f, 0.1f, 0.1f, NodeEditorPrefs.minimapOpacity), NodeEditorPrefs.minimapOutlineColor);

            GUILayout.BeginArea(boxPos);

            Rect localRect = new Rect(0f, 0f, boxPos.width, boxPos.height);

            Color nodeOutlineColor = new Color(0.15f, 0.15f, 0.15f, 1f);

            bool hoveredNode = false;
            for (int i = target.nodes.Length - 1; i >= 0; i--)
            {
                Node active = activeAgent?.GetRunningNode();

                Node node = target.nodes[i];

                Vector2 size = GetAreaWithPadding(node, false) / viewRect.width * viewWidth;
                Vector2 position = GridToMinimapPosition(node.position, viewWidth, minimapPadding);
                position.x += boxPos.width / 2f - viewWidth / 2f;

                foreach (Node child in node.children)
                {
                    bool isActiveConnection = (EditorApplication.isPlaying
                        || EditorApplication.isPaused)
                        && active != null
                        && IsSubTreeOf(node, active)
                        && (child == active || IsSubTreeOf(child, active));

                    Vector2 childSize = GetAreaWithPadding(child, false) / viewRect.width * viewWidth;
                    Vector2 childPosition = GridToMinimapPosition(child.position, viewWidth, minimapPadding);
                    childPosition.x += boxPos.width / 2f - viewWidth / 2f;

                    Color c = Handles.color;
                    Handles.color = isActiveConnection ? NodeEditorPrefs.highlightColor : Color.white;
                    Handles.DrawAAPolyLine(new Vector2(position.x + size.x / 2f, position.y + size.y), new Vector2(childPosition.x + childSize.x / 2f, childPosition.y));
                    Handles.color = c;
                }

                Rect nodeRect = new Rect(position, size);

                Color nodeColor;

                if (windowInfo.selected.Contains(node))
                    nodeColor = Color.white;
                else if (active == node)
                    nodeColor = NodeEditorPrefs.highlightColor;
                else
                    nodeColor = nodeOutlineColor;

                Handles.DrawSolidRectangleWithOutline(nodeRect, Styles.windowBackground, nodeColor);

                if (nodeRect.Contains(Event.current.mousePosition))
                {
                    windowInfo.hoveredNode = node;
                    windowInfo.hoveredType = Window.Hovering.MinimapNode;
                    hoveredNode = true;
                }
                else if (!hoveredNode && localRect.Contains(Event.current.mousePosition))
                {
                    windowInfo.hoveredType = Window.Hovering.Minimap;
                }
            }

            Handles.DrawSolidRectangleWithOutline(gridViewRect, new Color(0f, 0f, 0f, 0f), Color.gray);

            string names = String.Join(", ", windowInfo.selected.Select(node => node.name));
            if (names.Length > 35)
                names = names.Substring(0, 35) + "...";
            GUIContent content = new GUIContent(names);
            Vector2 contentSize = EditorStyles.miniLabel.CalcSize(content);
            GUI.Label(new Rect(boxPos.width - contentSize.x - 5f, 5f, contentSize.x, contentSize.y), content, EditorStyles.miniLabel);

            GUILayout.EndArea();

            windowInfo.minimapView = gridViewRect;
        }
        private void DrawToolbar()
        {
            Rect toolbar = new Rect(0f, 0f, window.width, EditorStyles.toolbar.fixedHeight);
            GUI.Box(toolbar, "", EditorStyles.toolbar);

            GUI.color = Color.white;
            GUILayout.BeginArea(toolbar, EditorStyles.toolbar);
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Add Node", EditorStyles.toolbarButton))
            {
                windowInfo.searchWantsNode = true;
                ToggleSearch();
            }

            List<float> f = new List<float>();

            if (GUILayout.Button("Add Decorator", EditorStyles.toolbarButton))
            {
                windowInfo.searchWantsNode = false;
                ToggleSearch();
            }

            if (GUILayout.Button("Prettify", EditorStyles.toolbarButton))
            {
                GraphUtility.Prettify(target.nodes);
                GetViewRect(100f, true);
            }

            GUILayout.FlexibleSpace();

            windowInfo.useLiveLink = GUILayout.Toggle(windowInfo.useLiveLink, "Live Link", EditorStyles.toolbarButton);

            windowInfo.drawMinimap = GUILayout.Toggle(windowInfo.drawMinimap, "Minimap", EditorStyles.toolbarButton);

            if (!windowInfo.inspectorToggled && GUILayout.Button(Styles.visibilityToggleOffContent, EditorStyles.toolbarButton))
                windowInfo.inspectorToggled = true;

            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        ///<summary>
        ///Draws the inspector for either the selected node or decorator
        ///</summary>
        private void DrawInspector()
        {
            if (!windowInfo.inspectorToggled)
                return;

            float inspectorWidth = windowInfo.inspectorWidth;
            Rect inspectorArea = new Rect(position.width - (inspectorWidth + GUIData.sidebarPadding * 2), 0f, inspectorWidth + GUIData.sidebarPadding * 2, position.height);

            Rect divider = new Rect(inspectorArea.x - 1f, EditorStyles.toolbar.fixedHeight, 1f, position.height - EditorStyles.toolbar.fixedHeight);
            Rect dividerRegion = new Rect(divider.x - 4.5f, divider.y, 10f, position.height);

            EditorGUI.DrawRect(inspectorArea, Styles.windowBackground);

            if (dividerRegion.Contains(Event.current.mousePosition))
            {
                windowInfo.hoveredType = Window.Hovering.Inspector;

                windowInfo.hoverDivider = true;

                if (Event.current.type == EventType.MouseDown)
                {
                    windowInfo.resizingInspector = true;
                    windowInfo.resizeClickOffset = Event.current.mousePosition.x - divider.x;
                }
            }
            else
            {
                windowInfo.hoverDivider = false;
            }

            if (windowInfo.resizingInspector)
            {
                float desired = Screen.width - Event.current.mousePosition.x - GUIData.sidebarPadding * 2 + windowInfo.resizeClickOffset;
                windowInfo.inspectorWidth = desired;
            }

            EditorGUI.DrawRect(divider, Styles.windowAccent);

            if (IsNotLayoutEvent(Event.current) && inspectorArea.Contains(Event.current.mousePosition)) windowInfo.hoveredType = Window.Hovering.Inspector;

            if (!windowInfo.settingsShown)
            {
                Rect inspectorContainer = new Rect(
                    position.width - inspectorWidth - GUIData.sidebarPadding * 2,
                    0f,
                    inspectorWidth + GUIData.sidebarPadding * 2,
                    position.height
                );

                GUILayout.BeginArea(inspectorContainer);
                GUILayout.BeginHorizontal(EditorStyles.toolbar);

                string[] values = Enum.GetNames(typeof(Window.InspectorView));

                GUILayout.Space(10);

                for (int i = 0; i < values.Length; i++)
                {
                    if (GUILayout.Toggle((int)windowInfo.inspectorView == i, values[i], EditorStyles.toolbarButton, GUILayout.Width(100)))
                        windowInfo.inspectorView = (Window.InspectorView)i;
                }

                GUILayout.FlexibleSpace();

                if (GUILayout.Button(
                    Styles.visibilityToggleOnContent,
                    EditorStyles.toolbarButton
                )) windowInfo.inspectorToggled = false;

                GUILayout.EndHorizontal();

                windowInfo.inspectorScroll = GUILayout.BeginScrollView(windowInfo.inspectorScroll);
                GUILayout.BeginHorizontal();
                GUILayout.Space(GUIData.nodePadding);
                GUILayout.BeginVertical();

                switch (windowInfo.inspectorView)
                {
                    case Window.InspectorView.Inspector:
                        DrawInspectorWindow();
                        break;
                    case Window.InspectorView.Blackboard:
                        DrawBlackboard(target.blackboard);
                        break;
                }
                GUILayout.EndVertical();
                GUILayout.Space(GUIData.nodePadding);
                GUILayout.EndHorizontal();
                GUILayout.EndScrollView();
                GUILayout.EndArea();
            }
            else
            {
                Rect prefsWindow = new Rect(position.width - inspectorWidth - GUIData.sidebarPadding * 2f, 0f, inspectorWidth + GUIData.sidebarPadding * 2f, position.height);

                GUILayout.BeginArea(prefsWindow);

                GUILayout.BeginHorizontal(EditorStyles.toolbar);

                GUILayout.FlexibleSpace();

                if (GUILayout.Button(
                    Styles.visibilityToggleOnContent,
                    EditorStyles.toolbarButton
                )) windowInfo.inspectorToggled = false;

                GUILayout.EndHorizontal();

                windowInfo.inspectorScroll = GUILayout.BeginScrollView(windowInfo.inspectorScroll);
                GUILayout.BeginHorizontal();
                GUILayout.Space(GUIData.nodePadding);
                GUILayout.BeginVertical();
                DrawPreferencesWindow();
                GUILayout.EndVertical();
                GUILayout.Space(GUIData.nodePadding);
                GUILayout.EndHorizontal();
                GUILayout.EndScrollView();
                GUILayout.EndArea();
            }
        }
        void DrawPreferencesWindow()
        {
            GUILayout.Space(GUIData.sidebarPadding);
            GUILayout.Label("Preferences", Styles.styles.title);
            GUILayout.Space(GUIData.sidebarPadding);

            EditorGUILayout.LabelField("General", EditorStyles.boldLabel);
            NodeEditorPrefs.saveOnClose = EditorGUILayout.Toggle("Save on Close", NodeEditorPrefs.saveOnClose);
            NodeEditorPrefs.formatOnSave = EditorGUILayout.Toggle("Format on Save", NodeEditorPrefs.formatOnSave);
            NodeEditorPrefs.screenshotPath = EditorGUILayout.TextField("Screenshot Path", NodeEditorPrefs.screenshotPath);

            EditorGUILayout.LabelField("");

            EditorGUILayout.LabelField("Editor", EditorStyles.boldLabel);
            NodeEditorPrefs.selectionColor = EditorGUILayout.ColorField(
                new GUIContent("Selection Color", "The selection color to use for nodes"),
                NodeEditorPrefs.selectionColor
            );
            NodeEditorPrefs.highlightColor = EditorGUILayout.ColorField(
                new GUIContent("Highlight Color", "The color to use when highlighting a node"),
                NodeEditorPrefs.highlightColor
            );
            NodeEditorPrefs.enableStatusIndicators = EditorGUILayout.Toggle(
                new GUIContent("Enable Status Indicators", "Toggle status indicators for all nodes"),
                NodeEditorPrefs.enableStatusIndicators
            );
            NodeEditorPrefs.successColor = EditorGUILayout.ColorField(
                new GUIContent("Success Color", "Color to use when successful"),
                NodeEditorPrefs.successColor
            );
            NodeEditorPrefs.failureColor = EditorGUILayout.ColorField(
                new GUIContent("Failure Color", "Color to use when failed"),
                NodeEditorPrefs.failureColor
            );

            EditorGUILayout.LabelField("");

            EditorGUILayout.LabelField("Minimap", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Minimap Position");
            NodeEditorPrefs.minimapPosition = GUILayout.Toolbar(NodeEditorPrefs.minimapPosition, new string[] { "Bottom Left", "Top Left", "Bottom Right", "Top Right" });
            NodeEditorPrefs.minimapWidth = EditorGUILayout.FloatField("Minimap Width", NodeEditorPrefs.minimapWidth);
            NodeEditorPrefs.maxMinimapHeight = EditorGUILayout.FloatField("Max Minimap Height", NodeEditorPrefs.maxMinimapHeight);
            NodeEditorPrefs.minimapOpacity = EditorGUILayout.Slider("Minimap Opacity", NodeEditorPrefs.minimapOpacity, 0f, 1f);
            NodeEditorPrefs.minimapOutlineColor = EditorGUILayout.ColorField("Minimap Outline Color", NodeEditorPrefs.minimapOutlineColor);

            EditorGUILayout.LabelField("");
            EditorGUILayout.LabelField("Debug", EditorStyles.boldLabel);
            NodeEditorPrefs.enableDebugView = EditorGUILayout.Toggle("Enable Debug View", NodeEditorPrefs.enableDebugView);

            EditorGUILayout.LabelField("");

            if (GUILayout.Button("Reset to default"))
                NodeEditorPrefs.ResetToDefault();
        }

        ///<summary>
        ///Where the drawing of the inspector takes place
        ///</summary>
        void DrawInspectorWindow()
        {
            GUILayout.Space(GUIData.sidebarPadding);
            GUILayout.Label("Inspector", Styles.styles.title);
            GUILayout.Space(GUIData.sidebarPadding);

            if (distinctTypes.Count > 1)
            {
                EditorGUILayout.LabelField("Different Node Types Selected");
                GUILayout.Label("");

                foreach (Type t in distinctTypes) EditorGUILayout.LabelField(t.Name);
                return;
            }

            if (editor != null)
            {
                bool isInspectingDecorator = editor.targets.OfType<Decorator>().Count() > 0;

                EditorGUI.BeginChangeCheck();
                EditorGUI.BeginDisabledGroup(Application.isPlaying);
                if (isInspectingDecorator)
                {
                    defaultDecoratorEditor.OnInspectorGUI();
                    EditorGUILayout.LabelField("");
                }
                else if (editor.targets.All(obj => typeof(Node).IsAssignableFrom(obj.GetType())))
                {
                    defaultNodeEditor.OnInspectorGUI();
                    EditorGUILayout.LabelField("");
                }
                EditorGUILayout.LabelField(editor.targets[0].name, EditorStyles.boldLabel);
                editor.OnInspectorGUI();
                EditorGUI.EndDisabledGroup();
                if (EditorGUI.EndChangeCheck())
                {
                    if (editor.targets.OfType<BlackboardEntry>().Any()) { }
                    else if (isInspectingDecorator)
                    {
                        Decorator decorator = ((Decorator)editor.targets.ToList().Find(obj => (typeof(Decorator)).IsAssignableFrom(obj.GetType())));
                        windowInfo.changedNodes.Enqueue(decorator.node);
                    }
                    else
                    {
                        foreach (Node node in editor.targets)
                            windowInfo.changedNodes.Enqueue(node);
                    }

                    SceneView.RepaintAll();
                }
            }
        }
        void DrawBlackboard(Blackboard blackboard)
        {
            EditorGUI.BeginDisabledGroup(editingPaused);
            GUILayout.Space(GUIData.sidebarPadding);
            GUILayout.Label("Blackboard", Styles.styles.title);
            GUILayout.Space(GUIData.sidebarPadding);

            GUILayout.Space(GUIData.sidebarPadding);
            if (!blackboardEditor || blackboardEditor.target != blackboard)
                UnityEditor.Editor.CreateCachedEditor(blackboard, typeof(BlackboardEditor), ref blackboardEditor);
            blackboardEditor.OnInspectorGUI();

            EditorGUI.EndDisabledGroup();
        }
        private void ToggleSearch()
        {
            windowInfo.searchIsShown = !windowInfo.searchIsShown;
            windowInfo.searchRect.position = new Vector2(window.size.x / 2f - windowInfo.searchRect.width / 2f, window.size.y / 2f - windowInfo.searchRect.height / 2f);
            windowInfo.searchText = "";
            GUI.FocusControl("");

            if (windowInfo.searchIsShown)
            {
                searchWantsFocus = true;
                shouldFocusSearch = false;
            }
            else
            {
                searchWantsFocus = false;
                shouldFocusSearch = false;
            }
        }
        private Rect RenderSearch(Rect rect)
        {
            BeginWindows();

            Rect r = GUILayout.Window(1, rect, DoSearchWindow, "", Styles.styles.addNodeWindow);

            EndWindows();

            return r;
        }
        private void DoSearchWindow(int winID)
        {
            bool mouseClick = false;

            List<Type> results = GetSearchResults(windowInfo.searchText);
            Type current = results.Find(t => GUI.GetNameOfFocusedControl().Equals(t.Name));
            if (results.Count > 0)
                current ??= results[0];

            switch (Event.current.type)
            {
                //Event checks must be done inside the window function
                case EventType.KeyDown when Event.current.keyCode == KeyCode.Escape:
                    ToggleSearch();
                    break;
                case EventType.KeyDown when Event.current.keyCode == KeyCode.Return:
                    {
                        if (results.Count > 0)
                        {
                            if (windowInfo.searchWantsNode)
                                target.AddNode(current, WindowToGridPosition(window.size * 0.5f));
                            else
                                target.AddDecorators(windowInfo.selected, current);
                        }
                        ToggleSearch();
                        break;
                    }
                case EventType.KeyDown when Event.current.keyCode == KeyCode.UpArrow:
                    if (results.Count > 0)
                    {
                        int nextIndex = results.IndexOf(current) - 1;

                        nextIndex = Mathf.Clamp(nextIndex, 0, results.Count - 1);

                        GUI.FocusControl(results[nextIndex].Name);
                    }

                    break;
                case EventType.KeyDown when Event.current.keyCode == KeyCode.DownArrow:
                    if (results.Count > 0)
                    {
                        int nextIndex = results.IndexOf(current) + 1;

                        nextIndex = Mathf.Clamp(nextIndex, 0, results.Count - 1);

                        GUI.FocusControl(results[nextIndex].Name);
                    }

                    break;
                case EventType.MouseDown when Event.current.button == 0:
                    mouseClick = true;
                    break;
            }

            GUILayout.BeginVertical(Styles.styles.backgroundBg);

            GUILayout.Label(windowInfo.searchWantsNode ? "Add Node" : "Add Decorator", EditorStyles.whiteLargeLabel);

            GUILayout.BeginHorizontal(GUILayout.Height(32));
            GUILayout.Space(8);
            GUILayout.BeginVertical(GUILayout.Width(24), GUILayout.Height(32));
            GUILayout.FlexibleSpace();
            GUILayout.Label(new GUIContent(Styles.searchIcon), Styles.styles.searchbar, GUILayout.Height(24), GUILayout.Width(24));
            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
            GUILayout.Space(8);
            GUI.SetNextControlName("SearchTextField");
            windowInfo.searchText = GUILayout.TextField(windowInfo.searchText, Styles.styles.searchbar, GUILayout.ExpandHeight(true));
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            GUI.color = Color.white;

            bool resultIsFocused = results.Any(t => GUI.GetNameOfFocusedControl().Equals(t.Name));

            windowInfo.searchbarScroll = GUILayout.BeginScrollView(new Vector2(0f, windowInfo.searchbarScroll)).y;

            for (int i = 0; i < results.Count; i++)
            {
                Type t = results[i];
                bool thisResultIsFocused = GUI.GetNameOfFocusedControl().Equals(t.Name);

                if (resultIsFocused)
                    GUI.backgroundColor = thisResultIsFocused ? GUI.skin.settings.selectionColor : new Color(0f, 0f, 0f, 0f);
                else
                    GUI.backgroundColor = i == 0 ? GUI.skin.settings.selectionColor : new Color(0f, 0f, 0f, 0f);

                GUI.SetNextControlName(t.Name);
                GUILayout.Box(t.Name, Styles.styles.searchResult, GUILayout.ExpandWidth(true));

                if (mouseClick && GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
                {
                    if (thisResultIsFocused)
                    {
                        if (windowInfo.searchWantsNode)
                        {
                            target.AddNode(t, WindowToGridPosition(window.size * 0.5f));
                        }
                        else
                        {
                            target.AddDecorators(windowInfo.selected, t);
                            windowInfo.selected.ForEach(n => windowInfo.changedNodes.Enqueue(n));
                        }
                        ToggleSearch();
                    }
                    else
                    {
                        GUI.FocusControl(t.Name);
                    }
                }

                GUI.backgroundColor = Color.white;
            }

            GUILayout.EndScrollView();

            GUI.DragWindow();
        }
        void DoSplashWindow(int winID)
        {
            if (windowInfo == null) return;

            Event current = Event.current;
            Vector2 mousePosition = Event.current.mousePosition;
            bool mouseClick = false;

            if (current.type == EventType.MouseDown && current.button == 0)
            {
                mouseClick = true;
            }

            Texture2D splash = Styles.splashImage;
            float scale = 0.5f;

            Rect splashRect = GUILayoutUtility.GetRect(splash.width * scale, splash.height * scale);
            GUI.DrawTexture(splashRect, splash);

            GUILayout.Label("Recent Files", EditorStyles.whiteLargeLabel);

            string[] recent = EditorPrefs.GetString("Schema Recently Opened", "").Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            Dictionary<string, string> recentFiles = new Dictionary<string, string>();

            //Try to convert them to file names. If this fails, it means the file does not exist anymore
            for (int j = 0; j < recent.Length; j++)
            {
                string n = System.IO.Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(recent[j]));
                recentFiles.Add(recent[j], n);
            }

            windowInfo.splashScroll = GUILayout.BeginScrollView(new Vector2(0f, windowInfo.splashScroll)).y;
            foreach (KeyValuePair<string, string> s in recentFiles)
            {
                if (GUI.GetNameOfFocusedControl().Equals(s.Key))
                    GUI.backgroundColor = GUI.skin.settings.selectionColor;
                else
                    GUI.backgroundColor = new Color(0, 0, 0, 0);

                GUI.SetNextControlName(s.Key);
                GUILayout.Box(s.Value, Styles.styles.searchResult, GUILayout.ExpandWidth(true));

                if (mouseClick && GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
                {
                    GUI.FocusControl(s.Key);
                }
            }
            GUILayout.EndScrollView();

            GUI.backgroundColor = Color.white;

            GUILayout.FlexibleSpace();

            GUILayout.BeginVertical(GUILayout.Height(EditorStyles.toolbar.fixedHeight));

            GUILayout.FlexibleSpace();

            GUILayout.BeginHorizontal();

            EditorGUI.BeginDisabledGroup(!recentFiles.Keys.Contains(GUI.GetNameOfFocusedControl()));
            if (GUILayout.Button("Load", GUILayout.Width(100f)))
                OpenGraph(AssetDatabase.LoadAssetAtPath<Graph>(AssetDatabase.GUIDToAssetPath(GUI.GetNameOfFocusedControl())));
            EditorGUI.EndDisabledGroup();

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("New Graph", GUILayout.Width(100f)))
            {
                // Graph graph = NodeEditorFileHandler.CreateNew();
                // OpenGraph(graph);
            }

            GUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();

            GUILayout.EndVertical();
        }
        List<Type> GetSearchResults(string query)
        {
            string[] words = query.Trim().Split(' ');

            List<Type> types = windowInfo.searchWantsNode ? nodeTypes.Values.SelectMany(x => x).ToList() : decoratorTypes.ToList();

            if (String.IsNullOrEmpty(query))
                return types;

            List<Type> ret = new List<Type>();

            foreach (Type t in types)
            {
                for (int i = 0; i < words.Length; i++)
                {
                    string word = words[i];

                    if (t.Name.ToLower().Contains(word.ToLower()) && !ret.Contains(t))
                    {
                        ret.Add(t);
                    }
                }
            }

            ret = ret.OrderBy(t => StringSimilarity(t.Name, query)).ToList();

            return ret;
        }
        int StringSimilarity(string s, string t)
        {
            if (string.IsNullOrEmpty(s))
            {
                if (string.IsNullOrEmpty(t))
                    return 0;
                return t.Length;
            }

            if (string.IsNullOrEmpty(t))
            {
                return s.Length;
            }

            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];

            // initialize the top and right of the table to 0, 1, 2, ...
            for (int i = 0; i <= n; d[i, 0] = i++) ;
            for (int j = 1; j <= m; d[0, j] = j++) ;

            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                    int min1 = d[i - 1, j] + 1;
                    int min2 = d[i, j - 1] + 1;
                    int min3 = d[i - 1, j - 1] + cost;
                    d[i, j] = Math.Min(Math.Min(min1, min2), min3);
                }
            }
            return d[n, m];
        }
        ///<summary>
        ///Helper functions to zoom and pan the UI (taken from the XNode framework with some modifications)
        ///</summary>
        public void BeginZoomed(Rect rect, float zoom, float topPadding)
        {
            prevMatrix = GUI.matrix;
            GUI.EndClip();

            GUIUtility.ScaleAroundPivot(Vector2.one / zoom, rect.size * 0.5f);
            Vector4 padding = new Vector4(0, topPadding, 0, 0);
            padding *= zoom;
            GUI.BeginClip(new Rect(-((rect.width * zoom) - rect.width) * 0.5f, -(((rect.height * zoom) - rect.height) * 0.5f) + (topPadding * zoom),
                rect.width * zoom,
                rect.height * zoom));
        }

        ///<summary>
        ///Helper functions to zoom and pan the UI (taken from the XNode framework with some modification)
        ///</summary>
        public void EndZoomed()
        {
            GUI.EndClip();
            GUI.BeginClip(new Rect(0f, tabHeight, position.width, position.height));
            GUI.matrix = prevMatrix;
        }

        /// <summary>
        /// Pans the view smoothly over time
        /// </summary>
        /// <param name="to">Position to pan to (in scaled coordinates)</param>
        /// <param name="duration">How long, in seconds, it takes to pan to the given point</param>
        public void PanView(Vector2 to, float duration)
        {
            windowInfo.panDuration = duration;
            windowInfo.recordedTime = Time.realtimeSinceStartup;
            windowInfo.nextPan = to;

            needsPan = true;
        }

        ///<summary>
        ///Draws the grid to the screen based on zoom and pan
        ///</summary>
        public void DrawGrid(Rect rect, float zoom, Vector2 panOffset)
        {
            rect.position = Vector2.zero;

            Vector2 center = rect.size * .5f;
            Texture2D gridTex = Styles.gridTexture;
            Texture2D crossTex = Styles.crossTexture;

            // Offset from origin in tile units
            float xOffset = -(center.x * zoom + panOffset.x) / gridTex.width;
            float yOffset = ((center.y - rect.size.y) * zoom + panOffset.y) / gridTex.height;

            Vector2 tileOffset = new Vector2(xOffset, yOffset);

            // Amount of tiles
            float tileAmountX = Mathf.Round(rect.size.x * zoom) / gridTex.width;
            float tileAmountY = Mathf.Round(rect.size.y * zoom) / gridTex.height;

            Vector2 tileAmount = new Vector2(tileAmountX, tileAmountY);

            // Draw tiled background
            GUI.DrawTextureWithTexCoords(rect, gridTex, new Rect(tileOffset, tileAmount));
            GUI.DrawTextureWithTexCoords(rect, crossTex, new Rect(tileOffset + new Vector2(0.5f, 0.5f), tileAmount));
        }

        [Serializable]
        internal class Window
        {
            public NodeEditor editor;
            private float _zoom = 1f;
            public float zoom
            {
                get
                {
                    if (editor.target)
                        editor.target.zoom = _zoom;
                    return _zoom;
                }
                set
                {
                    float val = Mathf.Clamp(value, 1f, 2.5f);
                    _zoom = val;
                    if (editor.target)
                        editor.target.zoom = val;
                }
            }
            public Dictionary<string, float> alpha = new Dictionary<string, float>();
            public Dictionary<string, bool?> nodeStatus;
            public bool isPanning;
            public Vector2 nextPan;
            public float recordedTime;
            public float timeLastFrame;
            public float deltaTime => Time.realtimeSinceStartup - timeLastFrame;
            public float panDuration;
            private Vector2 _pan;
            public Vector2 pan
            {
                get
                {
                    //Remove inconsistencies between the target and actual value (if undo occurs, for example)
                    if (editor.target)
                        editor.target.pan = _pan;
                    return _pan;
                }
                set
                {
                    _pan = value;
                    if (editor.target)
                        editor.target.pan = value;
                }
            }
            public Vector2 mouseDownPos;
            public List<Node> selected = new List<Node>();
            public Queue<Node> changedNodes = new Queue<Node>();
            public Decorator selectedDecorator;
            public Node hoveredNode;
            public Decorator hoveredDecorator;
            public Hovering hoveredType;
            public Node hoveredConnection;
            public bool shouldCheckConnectionHover;
            public Hovering lastClicked;
            public bool didDragSinceMouseUp;
            public int hoveredDecoratorIndex;
            public Rect viewRect;
            public Vector2 blackboardScroll;
            public Vector2 inspectorScroll;
            public InspectorView inspectorView;
            public bool inspectorToggled = true;
            public bool searchIsShown;
            public bool searchWantsNode = true;
            public bool searchAddChildren;
            public float searchbarScroll = 0f;
            public string searchText;
            public float splashScroll;
            public Rect minimapView;
            public bool settingsShown;
            public bool useLiveLink;
            private float _inspectorWidth = 350f;
            public float inspectorWidth
            {
                get
                {
                    return _inspectorWidth;
                }
                set
                {
                    _inspectorWidth = Mathf.Clamp(value, 350f, editor.position.width - 100f);
                }
            }
            public bool resizingInspector;
            public bool hoverDivider;
            public float resizeClickOffset;
            public bool drawMinimap = true;
            [SerializeField] internal Rect searchRect = new Rect(0f, 0f, 250f, 350f);
            public enum Hovering
            {
                Node,
                InConnection,
                OutConnection,
                Decorator,
                Inspector,
                Window,
                Minimap,
                MinimapNode,
                None
            }
            public enum InspectorView
            {
                Inspector,
                Blackboard
            }
        }
        ///<summary>
        ///Contains utility info for the GUI
        ///</summary>
        [Serializable]
        internal static class GUIData
        {
            public static SerializableDictionary<Node, Vector2> sizes = new SerializableDictionary<Node, Vector2>();
            public static readonly float nodePadding = 15f;
            public static readonly float sidebarPadding = 15f;
            public static readonly float labelHeight = 30f;
            public static readonly float textHeight = 15f;
            public static readonly float spacing = 10f;
            public static readonly float minContentHeight = 75f;
            public static readonly float zoomSpeed = .05f;
            public static float inspectorWidth = 400f;
        }
    }
}