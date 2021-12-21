using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using Schema.Runtime;
using System.Linq;

namespace Schema.Editor
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
                if (Event.current.type == EventType.Layout) LayoutGUI();

                window = new Rect(0f, 0f, position.width - GUIData.inspectorWidth - GUIData.sidebarPadding * 2, position.height);

                DrawGrid(window, windowInfo.zoom, windowInfo.pan);
                DrawConnections();
                DrawNodes();
                DrawWindow();
                DrawMinimap();
                DrawToolbar();
                DrawInspector();

                Vector2 size;

                if (windowInfo.selectedDecorator)
                {
                    //Draw legend here

                    size = EditorStyles.label.CalcSize(new GUIContent("Lower Priority Nodes"));
                    GUI.Label(new Rect(position.width - size.x - (GUIData.inspectorWidth + GUIData.sidebarPadding * 2) - 32f, position.height - size.y - 8f, size.x, size.y),
                     new GUIContent("Lower Priority Nodes"), EditorStyles.label);

                    GUI.color = NodeEditorResources.lowerPriorityColor;
                    GUI.Label(new Rect(position.width - (GUIData.inspectorWidth + GUIData.sidebarPadding * 2) - 24f, position.height - size.y / 2f - 16f, 16f, 16f),
                        "",
                        NodeEditorResources.styles.decorator);
                    GUI.color = Color.white;

                    float lastHeight = size.y + 16f;
                    size = EditorStyles.label.CalcSize(new GUIContent("Self Nodes"));

                    //Draw legend here
                    GUI.Label(new Rect(position.width - size.x - (GUIData.inspectorWidth + GUIData.sidebarPadding * 2) - 32f, position.height - lastHeight - size.y - 8f, size.x, size.y),
                     new GUIContent("Self Nodes"), EditorStyles.label);

                    GUI.color = NodeEditorResources.selfColor;
                    GUI.Label(new Rect(position.width - (GUIData.inspectorWidth + GUIData.sidebarPadding * 2) - 24f, position.height - lastHeight - size.y / 2f - 16f, 16f, 16f),
                        "",
                        NodeEditorResources.styles.decorator);
                    GUI.color = Color.white;
                }

                if (activeAgent != null)
                {
                    string content = "Viewing GameObject " + activeAgent.gameObject.name;
                    size = EditorStyles.boldLabel.CalcSize(new GUIContent(content));
                    GUI.Label(new Rect(8f, 32f, size.x, size.y), content, EditorStyles.boldLabel);
                }

                editingPaused = Application.isPlaying;

                if (editingPaused)
                {
                    string content = "In edit mode, only modification of Node and Decorator properties is allowed.";
                    size = EditorStyles.boldLabel.CalcSize(new GUIContent(content));
                    GUI.Label(new Rect(8f, position.height - size.y - 8f, size.x, size.y), content, EditorStyles.boldLabel);
                }

                ProcessEvents(Event.current);

                if (windowInfo.isPanning) SetCursor(MouseCursor.Pan);
                else if (windowInfo.hoveringDivider || windowInfo.movingDivider) SetCursor(MouseCursor.ResizeVertical);

                if (windowInfo.treeDirty)
                {
                    windowInfo.graphSaved = false;
                    titleContent = new GUIContent(target.name + "*");
                }

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
                GUILayout.Window(1, new Rect(position.width / 2f - width / 2f, position.height / 2f - height / 2f, width, height), DoSplashWindow, "", NodeEditorResources.styles.addNodeWindow);
                EndWindows();
            }

            Repaint();
        }

        //This method is called to initialize any elements that have a tendency to change between Event calls
        //We cache these things so that we can avoid Unity GUI errors
        void LayoutGUI()
        {
            List<UnityEngine.Object> targets = new List<UnityEngine.Object>();
            List<BlackboardEntry> duplicateEntries = new List<BlackboardEntry>();

            //Check for duplicate items in entries
            for (int i = 0; i < target.blackboard.entries.Count; i++)
            {
                for (int j = 0; j < target.blackboard.entries.Count; j++)
                {
                    if (j == i || duplicateEntries.Contains(target.blackboard.entries[j]))
                        continue;

                    if (target.blackboard.entries[i] == target.blackboard.entries[j])
                        duplicateEntries.Add(target.blackboard.entries[j]);
                }
            }

            foreach (BlackboardEntry e in duplicateEntries)
            {
                target.blackboard.entries[target.blackboard.entries.IndexOf(e)] = ScriptableObject.Instantiate(e);
            }

            if (blackboardEditor && ((BlackboardEditor)blackboardEditor).selectedIndex != -1)
            {
                target.blackboard.entries.RemoveAll(x => x == null);

                if (((BlackboardEditor)blackboardEditor).selectedIndex <= target.blackboard.entries.Count - 1)
                {
                    targets.Add(target.blackboard.entries[((BlackboardEditor)blackboardEditor).selectedIndex]);
                }
            }
            else if (windowInfo.selectedDecorator != null)
            {
                targets.Add(windowInfo.selectedDecorator);

                if (!defaultDecoratorEditor || defaultDecoratorEditor.target == null || defaultDecoratorEditor.target != windowInfo.selectedDecorator)
                    UnityEditor.Editor.CreateCachedEditor(windowInfo.selectedDecorator, typeof(DecoratorEditor), ref defaultDecoratorEditor);
            }
            else if (windowInfo.selected.Count > 0)
            {
                targets.AddRange(windowInfo.selected);
            }

            distinctTypes = targets.Select(x => x.GetType()).Distinct().ToList();

            if (distinctTypes.Count > 1) return;

            //caches inspector once per OnGUI call so we don't get EventType.Layout and EventType.Repaint related errors
            if (editor == null || editor.targets.Any(obj => obj == null) || !editor.targets.SequenceEqual(targets) && targets.Count > 0 && targets.All(x => x != null))
            {
                UnityEditor.Editor.CreateCachedEditor(targets.ToArray(), null, ref editor);
            }

            // ReSharper disable once Unity.NoNullPropagation
            SchemaAgent agent = Selection.activeGameObject?.GetComponent<SchemaAgent>();

            if (Application.isPlaying)
            {
                if (!activeAgent || (agent != null && agent != activeAgent && agent.target == original))
                {
                    activeAgent = agent;
                    editingPaused = true;
                }
            }
            else
            {
                if (agent)
                {
                    if (windowInfo.selectedDecorator)
                        agent.editorTarget = windowInfo.selectedDecorator.node;
                    else
                        agent.editorTarget = windowInfo.selected.Count > 0 ? windowInfo.selected[0] : null;
                }
                activeAgent = null;
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

                    Handles.DrawBezier(
                        @from,
                        new Vector2(to.x, to.y - 8f),
                        @from + Vector2.up * 50f,
                        to - Vector2.up * 50f,
                        Color.white,
                        null,
                        4f
                    );

                    Rect r = new Rect(to.x - 4f, to.y - 8f, 8f, 8f);

                    GUI.color = new Color(.85f, .85f, .85f, 1f);

                    GUI.DrawTexture(r, NodeEditorResources.arrow);
                }
            }

            if (requestingConnection != null)
            {
                Node node = requestingConnection;
                Vector2 nodeSize = GetArea(node, false);

                Vector2 from = GridToWindowPositionNoClipped(new Vector2(
                    node.position.x + (nodeSize.x + GUIData.nodePadding * 2) / 2f,
                    node.position.y + nodeSize.y + GUIData.nodePadding * 2 + 16f));
                Vector2 to = new Vector2(mousePos.x, mousePos.y - 8f);

                Handles.DrawBezier(
                    from,
                    to,
                    from + Vector2.up * 50f,
                    to - Vector2.up * 50f,
                    Color.white,
                    null,
                    4f
                );

                Rect r = new Rect(to.x - 4f, to.y, 8f, 8f);

                GUI.color = new Color(.85f, .85f, .85f, 1f);

                GUI.DrawTexture(r, NodeEditorResources.arrow);
            }
            EndZoomed();
        }

        ///<summary>
        ///Draws the node in the Node Editor Window, including decorators. Also handles node gizmos
        ///</summary>
        private void DrawNodes()
        {
            Event current = Event.current;

            //these will be overriden later if the nodes, decorators, or inspector contains the mouse position
            if (window.Contains(current.mousePosition) && IsNotLayoutEvent(current))
                windowInfo.hoveredType = Window.Hovering.Window;
            else if (IsNotLayoutEvent(current))
                windowInfo.hoveredType = Window.Hovering.None;

            BeginZoomed(window, windowInfo.zoom, tabHeight);

            List<Node> nodes = target.nodes;

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
                    {
                        NodeTicked(node);
                    }
                }

                for (int i = nodes.Count - 1; i >= 0; i--)
                {
                    Node node = nodes[i];
                    Vector2 positionNoClipped = GridToWindowPositionNoClipped(node.position);
                    Vector2 size = GetArea(node, node.dirty);
                    Vector2 sizeWithPadding = GetAreaWithPadding(node, node.dirty);
                    node.dirty = false;
                    node.graph = target;

                    Rect contained = new Rect(positionNoClipped.x + GUIData.nodePadding, positionNoClipped.y + GUIData.nodePadding, size.x, size.y);
                    Rect rect = new Rect(positionNoClipped, new Vector2(sizeWithPadding.x, sizeWithPadding.y));

                    GUI.color = NodeEditorResources.windowBackground;

                    GUI.Box(rect, "", NodeEditorResources.styles.node);

                    if (windowInfo.selected.Contains(node))
                        GUI.color = Color.white;
                    else if (windowInfo.alpha.ContainsKey(node.uID))
                        GUI.color = Color.Lerp(new Color32(80, 80, 80, 255), new Color32(247, 181, 0, 255), windowInfo.alpha[node.uID]);
                    else if (windowInfo.selectedDecorator &&
                        node.priority > 0 &&
                        windowInfo.selectedDecorator.node.priority > 0 &&
                        (windowInfo.selectedDecorator.abortsType == Decorator.ObserverAborts.Self ||
                        windowInfo.selectedDecorator.abortsType == Decorator.ObserverAborts.Both
                        ) &&
                        IsSubTreeOf(windowInfo.selectedDecorator.node, node))
                        GUI.color = NodeEditorResources.selfColor;
                    else if (windowInfo.selectedDecorator &&
                        node.priority > 0 &&
                        windowInfo.selectedDecorator.node.priority > 0 &&
                        (windowInfo.selectedDecorator.abortsType == Decorator.ObserverAborts.LowerPriority ||
                        windowInfo.selectedDecorator.abortsType == Decorator.ObserverAborts.Both
                        ) &&
                        IsLowerPriority(windowInfo.selectedDecorator.node, node) &&
                        windowInfo.selectedDecorator.node.priority < node.priority)
                        GUI.color = NodeEditorResources.lowerPriorityColor;
                    else if (EditorGUIUtility.isProSkin)
                        GUI.color = new Color32(80, 80, 80, 255);

                    GUI.Box(rect, "", NodeEditorResources.styles.nodeSelected);

                    GUI.color = Color.white;

                    bool blocked = false;

                    if (node.priority > 0)
                    {
                        Handles.color = NodeEditorResources.windowAccent;
                        Handles.DrawAAConvexPolygon(HelperMethods.Circle(new Vector2(rect.x, rect.center.y), 15f, 18));
                        Handles.color = Color.white;

                        GUI.Label(new Rect(rect.x - 15f, rect.center.y - 15f, 30f, 30f), node.priority.ToString(), NodeEditorResources.styles.title);
                    }

                    GUIContent error = GetErrors(node);

                    if (error != GUIContent.none)
                    {
                        float iconWidth = error.image.width;
                        float iconHeight = error.image.height;
                        GUI.Label(new Rect(rect.x + rect.width - iconWidth / 2f, rect.y + rect.height - iconHeight / 2f, iconWidth, iconHeight), error, GUIStyle.none);
                    }

                    GUILayout.BeginArea(contained);
                    GUILayout.BeginVertical();

                    List<float> positions = new List<float>();

                    for (int j = 0; j < node.decorators.Count; j++)
                    {
                        Decorator d = node.decorators[j];

                        bool isSelected = windowInfo.selectedDecorator == d;

                        GUI.color = isSelected ? new Color(.6f, .6f, .1f, 1f) : new Color(.1f, .1f, .4f, 1f);

                        GUILayout.BeginVertical(NodeEditorResources.styles.decorator);

                        GUI.color = Color.white;

                        GUILayout.Label(d.Name, NodeEditorResources.styles.nodeLabel, GUILayout.Height(GUIData.labelHeight), GUILayout.Width(contained.width));

                        d.info ??= d.GetValuesGUI();

                        foreach (string s in d.info)
                        {
                            GUILayout.Label(s, NodeEditorResources.styles.nodeText, GUILayout.Height(GUIData.textHeight), GUILayout.Width(contained.width));
                        }

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

                        if (j == node.decorators.Count - 1)
                        {
                            //last item, add extra position to snap to
                            positions.Add(last.position.y + last.size.y + (GUIData.spacing / 2f));
                        }
                    }

                    Rect toDraw = new Rect();
                    bool draw = node.decorators.Count > 0 && (windowInfo.hoveredNode == node || windowInfo.hoveredType == Window.Hovering.Window) && IsNotLayoutEvent(Event.current);

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

                    GUI.color = NodeEditorResources.windowAccent;

                    float contentHeight = Mathf.Max((node.icon == null ? 0 : node.icon.height + 10f) + GUIData.labelHeight, GUIData.minContentHeight);

                    GUILayout.BeginVertical(NodeEditorResources.styles.decorator, GUILayout.Height(contentHeight));

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
                    GUILayout.Label(node.Name, NodeEditorResources.styles.nodeLabel, GUILayout.ExpandHeight(true));

                    GUILayout.EndVertical();

                    GUILayout.EndVertical();

                    GUILayout.EndArea();

                    if (draw && windowInfo.lastClicked == Window.Hovering.Decorator && windowInfo.didDragSinceMouseUp && node.decorators.Contains(windowInfo.selectedDecorator))
                    {
                        toDraw.position += contained.position;

                        EditorGUI.DrawRect(toDraw, new Color32(200, 200, 200, 255));
                    }

                    if (node.drawInConnection)
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
                            GUI.color = NodeEditorResources.windowAccent;

                        GUI.Box(inConnection, "", NodeEditorResources.styles.decorator);
                    }

                    if (node.drawOutConnection)
                    {
                        float width = size.x - GUIData.nodePadding * 2;
                        Rect outConnection = new Rect(positionNoClipped.x + GUIData.nodePadding + size.x / 2f - width / 2f, positionNoClipped.y + size.y + GUIData.nodePadding * 2, width, 16f);

                        if (outConnection.Contains(current.mousePosition) && node.canHaveChildren && IsNotLayoutEvent(current))
                        {
                            windowInfo.hoveredType = Window.Hovering.OutConnection;
                            windowInfo.hoveredNode = node;
                        }

                        if (windowInfo.hoveredType == Window.Hovering.OutConnection && windowInfo.hoveredNode == node && !drawBox)
                            GUI.color = Color.white;
                        else
                            GUI.color = NodeEditorResources.windowAccent;

                        GUI.Box(outConnection, "", NodeEditorResources.styles.decorator);
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
                    windowInfo.alpha[node] -= 0.5f * Time.deltaTime;

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
        private Vector2 GetArea(Node node, bool recalculate)
        {
            if (node == null) return Vector2.zero;

            //try to get from dictionary
            if (guiData.sizes.ContainsKey(node) && !recalculate)
            {
                return guiData.sizes[node];
            }

            //get size of contents
            float height = Mathf.Max((node.icon == null ? 0 : node.icon.height + 10f) + GUIData.labelHeight, GUIData.minContentHeight);
            float width = Mathf.Max(NodeEditorResources.styles.nodeLabel.CalcSize(new GUIContent(node.Name)).x, node.icon == null ? 0 : node.icon.width);

            foreach (Decorator decorator in node.decorators)
            {
                //initialize decorator info array
                decorator.info = decorator.GetValuesGUI();

                float decoratorLabelWidth = NodeEditorResources.styles.nodeLabel.CalcSize(new GUIContent(decorator.Name)).x;

                width = Mathf.Max(width, decoratorLabelWidth);
                height += GUIData.labelHeight;

                foreach (string s in decorator.info)
                {
                    height += GUIData.textHeight;
                    width = Mathf.Max(width, NodeEditorResources.styles.nodeText.CalcSize(new GUIContent(s)).x);
                }

                //The 4 is accounting for the area that GUILayout.BeginVertical adds when applying a background for decorators. Not sure why this happens.
                height += GUIData.spacing + 4;
            }

            Vector2 final = new Vector2(width + 40f, height);

            if (recalculate)
                guiData.sizes[node] = final;
            else
                guiData.sizes.Add(node, final);

            return final;
        }
        private Vector2 GetAreaWithPadding(Node node, bool recalculate)
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
        }
        private void DrawMinimap()
        {
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
                    boxPos = new Rect(position.width - (GUIData.inspectorWidth + GUIData.sidebarPadding * 2) - 10f - NodeEditorPrefs.minimapWidth, position.height - minimapHeight - 10f, NodeEditorPrefs.minimapWidth, minimapHeight);
                    break;
                case 3:
                    boxPos = new Rect(position.width - (GUIData.inspectorWidth + GUIData.sidebarPadding * 2) - 10f - NodeEditorPrefs.minimapWidth, EditorStyles.toolbar.fixedHeight + 10f, NodeEditorPrefs.minimapWidth, minimapHeight);
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
            for (int i = target.nodes.Count - 1; i >= 0; i--)
            {
                Node node = target.nodes[i];

                //I don't know if this is right
                Vector2 size = GetAreaWithPadding(node, false) / viewRect.width * viewWidth;
                Vector2 position = GridToMinimapPosition(node.position, viewWidth, minimapPadding);
                position.x += boxPos.width / 2f - viewWidth / 2f;

                Rect nodeRect = new Rect(position, size);

                Handles.DrawSolidRectangleWithOutline(nodeRect, NodeEditorResources.windowBackground, windowInfo.selected.Contains(node) ? Color.white : nodeOutlineColor);

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

            string names = String.Join(", ", windowInfo.selected.Select(node => node.Name));
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
            Rect toolbar = new Rect(0f, 0f, position.width - GUIData.inspectorWidth - GUIData.sidebarPadding * 2, EditorStyles.toolbar.fixedHeight);
            GUI.color = NodeEditorResources.windowAccent;
            GUI.Box(toolbar, "", EditorStyles.toolbar);

            GUI.color = Color.white;
            GUILayout.BeginArea(toolbar, EditorStyles.toolbar);
            GUILayout.BeginHorizontal();

            GUILayout.Button("View", EditorStyles.toolbarButton);

            Rect r = GUILayoutUtility.GetRect(new GUIContent("Add"), EditorStyles.toolbarButton);
            if (GUI.Button(r, "Add", EditorStyles.toolbarButton))
                GenerateAddMenu().ShowAsContext(r.x, r.y + r.height);

            r = GUILayoutUtility.GetRect(new GUIContent("Node"), EditorStyles.toolbarButton);
            if (GUI.Button(r, "Node", EditorStyles.toolbarButton))
                GenerateNodeContextMenu().ShowAsContext(r.x, r.y + r.height);

            if (GUILayout.Button("Prettify", EditorStyles.toolbarButton))
                BeautifyTree(new Vector2(50f, 150f));

            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Load", EditorStyles.toolbarButton, GUILayout.Width(75f)))
                NodeEditorFileHandler.Load(this);
            if (GUILayout.Button("Save As", EditorStyles.toolbarButton, GUILayout.Width(75f)))
                NodeEditorFileHandler.SaveAs(this);
            if (GUILayout.Button("Save", EditorStyles.toolbarButton, GUILayout.Width(75f)))
                NodeEditorFileHandler.Save(this);

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Screenshot", EditorStyles.toolbarButton))
            {
                this.Screenshot();
            }

            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        ///<summary>
        ///Draws the inspector for either the selected node or decorator
        ///</summary>
        private void DrawInspector()
        {

            if (windowInfo.dividerPos < 0.0f) windowInfo.dividerPos = position.height / 2f;

            float inspectorWidth = GUIData.inspectorWidth;
            Rect inspectorArea = new Rect(position.width - (inspectorWidth + GUIData.sidebarPadding * 2), 0f, inspectorWidth + GUIData.sidebarPadding * 2, position.height);

            EditorGUI.DrawRect(inspectorArea, NodeEditorResources.windowBackground);

            if (IsNotLayoutEvent(Event.current) && inspectorArea.Contains(Event.current.mousePosition)) windowInfo.hoveredType = Window.Hovering.Inspector;

            if (!windowInfo.settingsShown)
            {
                //draw divider line
                Rect divider = new Rect(position.width - (inspectorWidth + GUIData.sidebarPadding * 2), windowInfo.dividerPos - GUIData.dividerHeight / 2f,
                    inspectorWidth + GUIData.sidebarPadding * 2, GUIData.dividerHeight);
                Rect dividerInput = new Rect(position.width - inspectorWidth, windowInfo.dividerPos - (GUIData.dividerHeight * 5 / 2f),
                    inspectorWidth, GUIData.dividerHeight * 5);
                EditorGUI.DrawRect(divider, NodeEditorResources.windowAccent);

                windowInfo.hoveringDivider =
                    IsNotLayoutEvent(Event.current) &&
                    dividerInput.Contains(Event.current.mousePosition);

                Rect inspector = new Rect(position.width - inspectorWidth - GUIData.sidebarPadding, 0f, inspectorWidth, windowInfo.dividerPos);
                GUILayout.BeginArea(inspector);
                windowInfo.inspectorScroll = GUILayout.BeginScrollView(windowInfo.inspectorScroll);
                DrawInspectorWindow();
                GUILayout.EndScrollView();
                GUILayout.EndArea();
                Rect blackboard = new Rect(position.width - inspectorWidth - GUIData.sidebarPadding,
                    windowInfo.dividerPos, inspectorWidth, position.height - windowInfo.dividerPos);
                GUILayout.BeginArea(blackboard);
                windowInfo.blackboardScroll = GUILayout.BeginScrollView(windowInfo.blackboardScroll);
                DrawBlackboard(target.blackboard);
                GUILayout.EndScrollView();
                GUILayout.EndArea();
            }
            else
            {
                Rect prefsWindow = new Rect(position.width - inspectorWidth - GUIData.sidebarPadding, 0f, inspectorWidth, position.height);

                GUILayout.BeginArea(prefsWindow);
                windowInfo.inspectorScroll = GUILayout.BeginScrollView(windowInfo.inspectorScroll);
                DrawPreferencesWindow();
                GUILayout.EndScrollView();
                GUILayout.EndArea();
            }
        }
        void DrawPreferencesWindow()
        {
            GUILayout.Space(GUIData.sidebarPadding);
            GUILayout.Label("Preferences", NodeEditorResources.styles.title);
            GUILayout.Space(GUIData.sidebarPadding);

            GUILayout.Label("General", EditorStyles.boldLabel);
            NodeEditorPrefs.saveOnClose = EditorGUILayout.Toggle("Save on Close", NodeEditorPrefs.saveOnClose);
            NodeEditorPrefs.formatOnSave = EditorGUILayout.Toggle("Format on Save", NodeEditorPrefs.formatOnSave);
            NodeEditorPrefs.screenshotPath = EditorGUILayout.TextField("Screenshot Path", NodeEditorPrefs.screenshotPath);

            GUILayout.Label("Minimap", EditorStyles.boldLabel);
            GUILayout.Label("Minimap Position");
            NodeEditorPrefs.minimapPosition = GUILayout.Toolbar(NodeEditorPrefs.minimapPosition, new string[] { "Bottom Left", "Top Left", "Bottom Right", "Top Right" });
            NodeEditorPrefs.minimapWidth = EditorGUILayout.FloatField("Minimap Width", NodeEditorPrefs.minimapWidth);
            NodeEditorPrefs.maxMinimapHeight = EditorGUILayout.FloatField("Max Minimap Height", NodeEditorPrefs.maxMinimapHeight);
            NodeEditorPrefs.minimapOpacity = EditorGUILayout.Slider("Minimap Opacity", NodeEditorPrefs.minimapOpacity, 0f, 1f);
            NodeEditorPrefs.minimapOutlineColor = EditorGUILayout.ColorField("Minimap Outline Color", NodeEditorPrefs.minimapOutlineColor);
        }

        ///<summary>
        ///Where the drawing of the inspector takes place
        ///</summary>
        void DrawInspectorWindow()
        {
            GUILayout.Space(GUIData.sidebarPadding);
            GUILayout.Label("Inspector", NodeEditorResources.styles.title);
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
                Dictionary<UnityEngine.Object, HideFlags> cache = new Dictionary<UnityEngine.Object, HideFlags>();

                for (int i = 0; i < editor.targets.Count(); i++)
                {
                    UnityEngine.Object target = editor.targets[i];
                    cache.Add(target, target.hideFlags);
                    target.hideFlags = HideFlags.None;
                }

                bool isInspectingDecorator = editor.targets.OfType<Decorator>().Count() > 0;

                EditorGUI.BeginChangeCheck();
                if (isInspectingDecorator)
                    defaultDecoratorEditor.OnInspectorGUI();
                editor.OnInspectorGUI();
                if (EditorGUI.EndChangeCheck())
                {
                    windowInfo.treeDirty = true;

                    if (editor.targets.OfType<BlackboardEntry>().Any()) { }
                    else if (isInspectingDecorator)
                    {
                        Decorator decorator = ((Decorator)editor.targets.ToList().Find(obj => (typeof(Decorator)).IsAssignableFrom(obj.GetType())));
                        decorator.node.dirty = true;
                        decorator.info = decorator.GetValuesGUI();
                    }
                    else
                    {
                        foreach (Node node in editor.targets)
                        {
                            node.dirty = true;
                        }
                    }
                }

                UpdateHideFlagsFromDictionary(cache);
            }
        }
        void DrawBlackboard(Blackboard blackboard)
        {
            EditorGUI.BeginDisabledGroup(editingPaused);
            GUILayout.Space(GUIData.sidebarPadding);
            GUILayout.Label("Blackboard", NodeEditorResources.styles.title);
            GUILayout.Space(GUIData.sidebarPadding);

            Dictionary<UnityEngine.Object, HideFlags> cache = new Dictionary<UnityEngine.Object, HideFlags> { { blackboard, blackboard.hideFlags } };

            blackboard.hideFlags = HideFlags.None;

            foreach (BlackboardEntry entry in blackboard.entries)
            {
                cache.Add(entry, entry.hideFlags);
                entry.hideFlags = HideFlags.None;
            }

            GUILayout.Space(GUIData.sidebarPadding);
            if (!blackboardEditor || blackboardEditor.target != blackboard)
                UnityEditor.Editor.CreateCachedEditor(blackboard, typeof(BlackboardEditor), ref blackboardEditor);
            EditorGUI.BeginChangeCheck();
            blackboardEditor.OnInspectorGUI();
            if (EditorGUI.EndChangeCheck())
            {
                windowInfo.treeDirty = true;
            }

            UpdateHideFlagsFromDictionary(cache);
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

            Rect r = GUILayout.Window(1, rect, DoSearchWindow, "", NodeEditorResources.styles.addNodeWindow);

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
                                AddNode(current, WindowToGridPosition(window.size * 0.5f), windowInfo.searchAddChildren);
                            else
                                AddDecorator(current);
                        }
                        ToggleSearch();
                        break;
                    }
                case EventType.KeyDown when Event.current.keyCode == KeyCode.UpArrow:
                    {
                        if (results.Count > 0)
                        {
                            int nextIndex = results.IndexOf(current) - 1;

                            nextIndex = Mathf.Clamp(nextIndex, 0, results.Count - 1);

                            GUI.FocusControl(results[nextIndex].Name);
                        }

                        break;
                    }
                case EventType.KeyDown:
                    {
                        if (Event.current.keyCode == KeyCode.DownArrow)
                        {
                            if (results.Count > 0)
                            {
                                int nextIndex = results.IndexOf(current) + 1;

                                nextIndex = Mathf.Clamp(nextIndex, 0, results.Count - 1);

                                GUI.FocusControl(results[nextIndex].Name);
                            }
                        }

                        break;
                    }
                case EventType.MouseDown when Event.current.button == 0:
                    mouseClick = true;
                    break;
            }

            GUILayout.BeginVertical(NodeEditorResources.styles.backgroundBg);

            Debug.Log(NodeEditorResources.styles.backgroundBg);

            GUILayout.Label(windowInfo.searchWantsNode ? "Add Node" : "Add Decorator", EditorStyles.whiteLargeLabel);

            GUILayout.BeginHorizontal(GUILayout.Height(32));
            GUILayout.Space(8);
            GUILayout.BeginVertical(GUILayout.Width(24), GUILayout.Height(32));
            GUILayout.FlexibleSpace();
            GUILayout.Label(new GUIContent(NodeEditorResources.searchIcon), NodeEditorResources.styles.searchbar, GUILayout.Height(24), GUILayout.Width(24));
            GUILayout.FlexibleSpace();
            GUILayout.EndVertical();
            GUILayout.Space(8);
            GUI.SetNextControlName("SearchTextField");
            windowInfo.searchText = GUILayout.TextField(windowInfo.searchText, NodeEditorResources.styles.searchbar, GUILayout.ExpandHeight(true));
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
                GUILayout.Box(t.Name, NodeEditorResources.styles.searchResult, GUILayout.ExpandWidth(true));

                if (mouseClick && GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
                {
                    if (thisResultIsFocused)
                    {
                        if (windowInfo.searchWantsNode)
                            AddNode(t, WindowToGridPosition(window.size * 0.5f), windowInfo.searchAddChildren);
                        else
                            AddDecorator(t);
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

            Texture2D splash = NodeEditorResources.splashImage;
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
                GUILayout.Box(s.Value, NodeEditorResources.styles.searchResult, GUILayout.ExpandWidth(true));

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
                Graph graph = NodeEditorFileHandler.CreateNew();
                OpenGraph(graph);
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
        void UpdateHideFlagsFromDictionary(Dictionary<UnityEngine.Object, HideFlags> cache)
        {
            foreach (KeyValuePair<UnityEngine.Object, HideFlags> kvp in cache)
            {
                if (kvp.Key == null)
                {
                    continue;
                }

                kvp.Key.hideFlags = kvp.Value;
            }
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
            /* GUIUtility.ScaleAroundPivot(Vector2.one * zoom, rect.size * 0.5f);
            Vector3 offset = new Vector3(
                (((rect.width * zoom) - rect.width) * 0.5f),
                (((rect.height * zoom) - rect.height) * 0.5f) + (-topPadding * zoom) + topPadding,
                0);
            GUI.matrix = Matrix4x4.TRS(offset, Quaternion.identity, Vector3.one); */
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
            Texture2D gridTex = NodeEditorResources.gridTexture;
            Texture2D crossTex = NodeEditorResources.crossTexture;

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

        ///<summary>
        ///Contains all the information about the window
        ///</summary>
        [Serializable]
        public class Window
        {
            ///<summary>
            ///The current NodeEditor that Window is describing
            ///</summary>
            public NodeEditor editor;
            private float _zoom;
            ///<summary>
            ///The current zoom level of the window
            ///</summary>
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
            ///<summary>
            ///Whether we are currently panning the view
            ///</summary>
            internal bool isPanning;
            internal Vector2 nextPan;
            internal float recordedTime;
            public float panDuration;
            private Vector2 _pan;
            ///<summary>
            ///The drag offset for the window
            ///</summary>
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

            ///<summary>
            ///Where the mouse last clicked (used for the selection box)
            ///</sumamry>
            public Vector2 mouseDownPos;

            ///<summary>
            ///The list of selected nodes in the window
            ///</summary>
            public List<Node> selected;

            ///<summary>
            ///Currently selected decorator
            ///</summary>
            public Decorator selectedDecorator;

            ///<summary>
            ///Node that is currently hovered
            ///</summary>
            public Node hoveredNode;

            ///<summary>
            ///Decorator that is currently hovered
            ///</summary>
            public Decorator hoveredDecorator;

            ///<summary>
            ///What we are currently hovering in the window
            ///</summary>
            public Hovering hoveredType;

            ///<summary>
            ///What we last clicked on mouse down
            ///</summary>
            public Hovering lastClicked;

            ///<summary>
            ///Whether the mouse has been dragged since we last released the mouse button
            ///</summary>
            public bool didDragSinceMouseUp;

            ///<summary>
            ///The hovered decorator list position, used to swap decorators by clicking and dragging
            ///</summary>
            public int hoveredDecoratorIndex;

            ///<summary>
            ///Whether we are hovering a divider
            ///</summary>
            public bool hoveringDivider;
            ///<summary>
            ///Whether we are currently moving a divider
            ///</summary>
            public bool movingDivider;
            ///<summary>
            ///Whether the current graph has been saved since the last edit
            ///</summary>
            public bool graphSaved;
            ///<summary>
            ///Whether the node tree was edited this frame (used in edit mode for realtime updates)
            ///</summary>
            public bool treeDirty;
            public Rect viewRect;
            private float _dividerPos;
            internal float dividerPos
            {
                get
                {
                    return Mathf.Clamp(_dividerPos, 50f, editor.position.height - 50f);
                }
                set
                {
                    _dividerPos = value;
                }
            }
            internal Vector2 blackboardScroll;
            internal Vector2 inspectorScroll;
            internal bool searchIsShown = false;
            internal bool searchWantsNode = true;
            internal bool searchAddChildren = false;
            internal float searchbarScroll = 0f;
            internal string searchText;
            internal float splashScroll;
            public Rect minimapView;
            public bool settingsShown;
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
        }
        ///<summary>
        ///Contains utility info for the GUI
        ///</summary>
        [Serializable]
        public class GUIData
        {
            public SerializableDictionary<Node, Vector2> sizes = new SerializableDictionary<Node, Vector2>();
            public static readonly float nodePadding = 15f;
            public static readonly float sidebarPadding = 15f;
            public static readonly float labelHeight = 30f;
            public static readonly float textHeight = 15f;
            public static readonly float spacing = 10f;
            public static readonly float minContentHeight = 75f;
            public static readonly float zoomSpeed = .05f;
            public static readonly float dividerHeight = 1.0f;
            public static float inspectorWidth = 400f;
        }
    }
}