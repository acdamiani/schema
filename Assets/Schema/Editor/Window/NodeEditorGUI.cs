using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Schema;
using Schema.Internal;
using Schema.Utilities;
using SchemaEditor.Editors;
using SchemaEditor.Internal;
using SchemaEditor.Internal.ComponentSystem;
using SchemaEditor.Internal.ComponentSystem.Components;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SchemaEditor
{
    public partial class NodeEditor
    {
        public Rect window;
        private Editor blackboardEditor;
        public ComponentCanvas canvas;
        private Editor defaultNodeEditor;
        private Editor defaultConditionalEditor;
        private List<Type> distinctTypes;
        private bool editingPaused;
        private Editor editor;
        private Func<bool> isDockedFunc;
        private bool needsPan;
        public float tabHeight => isDocked() ? 19.0f : 21.0f;
        private Func<bool> isDocked => isDockedFunc ??= this.GetIsDockedDelegate();

        private void OnGUI()
        {
            CalculateWindow();

            if (canvas != null)
                canvas.Draw();

            if (target != null && canvas != null)
            {
                CreateEditors();

                DrawInspector();
                DrawToolbar();

                Blackboard.instance = target.blackboard;
            }

            Repaint();
        }

        private void CreateEditors()
        {
            if (blackboardEditor == null)
                blackboardEditor = Editor.CreateEditor(target.blackboard, typeof(BlackboardEditor));

            if (editor != null && editor.targets.Any(x => !x))
                DestroyImmediate(editor);
            else if (defaultNodeEditor != null && defaultNodeEditor.targets.Any(x => !x))
                DestroyImmediate(defaultNodeEditor);
            else if (defaultConditionalEditor != null && defaultConditionalEditor.targets.Any(x => !x))
                DestroyImmediate(defaultConditionalEditor);

            List<Object> targets = new List<Object>();

            IEnumerable<Object> editableComponents;

            if (canvas == null)
            {
                editableComponents = Enumerable.Empty<Object>();
            }
            else
            {
                editableComponents = canvas.selected
                    .Where(x => x is IEditable)
                    .Cast<IEditable>()
                    .Where(x => x.IsEditable())
                    .Select(x => x.GetEditable());
            }

            targets = editableComponents.Where(x => x != null).ToList();
            distinctTypes = targets.Select(x => x.GetType()).Distinct().ToList();

            if (distinctTypes.Count > 1) return;

            if (editor == null)
            {
                if (targets.Count > 0)
                    editor = Editor.CreateEditor(targets.ToArray());
            }
            else if (!editor.targets.SequenceEqual(targets))
            {
                DestroyImmediate(editor);

                if (targets.Count > 0)
                    editor = Editor.CreateEditor(targets.ToArray());
            }

            if (targets.All(x => x is Node))
            {
                if (defaultNodeEditor == null)
                {
                    if (targets.Count > 0)
                        defaultNodeEditor = Editor.CreateEditor(targets.ToArray(), typeof(DefaultNodeEditor));
                }
                else if (!defaultNodeEditor.targets.SequenceEqual(targets))
                {
                    DestroyImmediate(defaultNodeEditor);

                    if (targets.Count > 0)
                        defaultNodeEditor = Editor.CreateEditor(targets.ToArray(), typeof(DefaultNodeEditor));
                }
            }
            else if (targets.All(x => x is Conditional))
            {
                if (defaultConditionalEditor == null)
                {
                    if (targets.Count > 0)
                        defaultConditionalEditor = Editor.CreateEditor(targets.ToArray(), typeof(DefaultConditionalEditor));
                }
                else if (!defaultConditionalEditor.targets.SequenceEqual(targets))
                {
                    DestroyImmediate(defaultConditionalEditor);

                    if (targets.Count > 0)
                        defaultConditionalEditor = Editor.CreateEditor(targets.ToArray(), typeof(DefaultConditionalEditor));
                }
            }
        }

        private void CalculateWindow()
        {
            if (windowInfo.inspectorToggled)
                window = new Rect(0f, 0f, position.width - Window.inspectorWidth - Window.padding * 2,
                    position.height);
            else
                window = new Rect(0f, 0f, position.width, position.height);
        }

        private void DrawToolbar()
        {
            Rect toolbar = new Rect(0f, 0f, window.width, EditorStyles.toolbar.fixedHeight);
            GUI.Box(toolbar, "", EditorStyles.toolbar);

            GUI.color = Color.white;
            GUILayout.BeginArea(toolbar, EditorStyles.toolbar);
            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Add Node", EditorStyles.toolbarButton))
                CreateAddNodeWindow();

            EditorGUI.BeginDisabledGroup(CanAddConditional());

            if (GUILayout.Button("Add Conditional", EditorStyles.toolbarButton))
                CreateAddConditionalWindow();

            EditorGUI.EndDisabledGroup();

            if (GUILayout.Button("Arrange", EditorStyles.toolbarButton))
                GraphUtility.Arrange(target.nodes);

            GUILayout.FlexibleSpace();

            Prefs.liveLink = GUILayout.Toggle(Prefs.liveLink, "Live Link", EditorStyles.toolbarButton);
            Prefs.minimapEnabled = GUILayout.Toggle(Prefs.minimapEnabled, "Minimap", EditorStyles.toolbarButton);
            Prefs.gridSnap = GUILayout.Toggle(Prefs.gridSnap, "Grid Snap", EditorStyles.toolbarButton);

            if (!windowInfo.inspectorToggled && GUILayout.Button(Icons.GetEditor("animationvisibilitytoggleoff"),
                    EditorStyles.toolbarButton))
                windowInfo.inspectorToggled = true;

            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        /// <summary>
        ///     Draws the inspector for either the selected node or decorator
        /// </summary>
        private void DrawInspector()
        {
            if (!windowInfo.inspectorToggled)
                return;

            float inspectorWidth = Window.inspectorWidth;
            Rect inspectorArea = new Rect(position.width - (inspectorWidth + Window.padding * 2), 0f,
                inspectorWidth + Window.padding * 2, position.height);

            Rect inspectorContainer = new Rect(
                position.width - inspectorWidth - Window.padding * 2,
                0f,
                inspectorWidth + Window.padding * 2,
                position.height
            );

            GUILayout.BeginArea(inspectorContainer);
            GUILayout.BeginHorizontal(EditorStyles.toolbar);

            string[] values = Enum.GetNames(typeof(Window.InspectorView));

            GUIContent[] content = new GUIContent[2]
                { new(values[0], Icons.GetEditor("UnityEditor.InspectorWindow")), new(values[1], Icons.GetEditor("UnityEditor.HierarchyWindow")) };

            GUILayout.FlexibleSpace();

            for (int i = 0; i < content.Length; i++)
                if (GUILayout.Toggle((int)windowInfo.inspectorView == i, content[i], EditorStyles.toolbarButton,
                        GUILayout.Width(100)))
                    windowInfo.inspectorView = (Window.InspectorView)i;

            GUILayout.FlexibleSpace();

            if (GUILayout.Button(
                    Icons.GetEditor("animationvisibilitytoggleon"),
                    EditorStyles.toolbarButton
                )) windowInfo.inspectorToggled = false;

            GUILayout.EndHorizontal();

            windowInfo.inspectorScroll = GUILayout.BeginScrollView(windowInfo.inspectorScroll);
            GUILayout.BeginHorizontal();
            GUILayout.Space(Window.padding);
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
            GUILayout.Space(Window.padding);
            GUILayout.EndHorizontal();
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        /// <summary>
        ///     Where the drawing of the inspector takes place
        /// </summary>
        private void DrawInspectorWindow()
        {
            if (distinctTypes.Count > 1)
            {
                EditorGUILayout.LabelField("Different Node Types Selected");
                GUILayout.Label("");

                foreach (Type t in distinctTypes) EditorGUILayout.LabelField(t.Name);
                return;
            }

            GUILayout.Space(8);

            if (editor != null)
            {
                EditorGUI.BeginChangeCheck();
                EditorGUI.BeginDisabledGroup(Application.isPlaying);

                EditorGUILayout.LabelField(editor.targets[0].name, EditorStyles.boldLabel);

                if (defaultNodeEditor != null)
                    defaultNodeEditor.OnInspectorGUI();
                else if (defaultConditionalEditor != null)
                    defaultConditionalEditor.OnInspectorGUI();

                editor.OnInspectorGUI();
                EditorGUI.EndDisabledGroup();
            }
        }

        private void DrawBlackboard(Blackboard blackboard)
        {
            blackboardEditor.OnInspectorGUI();
        }

        private void RebuildComponentTree()
        {
            if (target == null)
            {
                DoSplashCanvas();
                return;
            }

            if (canvas == null)
            {
                SelectionBoxComponent.SelectionBoxComponentCreateArgs sBoxCreateArgs =
                   new SelectionBoxComponent.SelectionBoxComponentCreateArgs();
                sBoxCreateArgs.hideOnMouseUp = true;

                MinimapComponent.MinimapComponentCreateArgs minimapCreateArgs =
                    new MinimapComponent.MinimapComponentCreateArgs();
                minimapCreateArgs.offset = () => new Vector2(0f, EditorStyles.toolbar.fixedHeight);

                PannerZoomer zoomer = new PannerZoomer(this, 0.05f, target.zoom, target.pan,
                    () => isDocked() ? 19.0f : 21.0f);

                zoomer.onPanChange += pan => target.pan = pan;
                zoomer.onZoomChange += zoom => target.zoom = zoom;

                canvas = new ComponentCanvas(this, sBoxCreateArgs, minimapCreateArgs, zoomer, DrawGrid);
            }

            IEnumerable<NodeComponent> nodeComponents = canvas.components
                .Where(x => x is NodeComponent)
                .Cast<NodeComponent>();

            IEnumerable<ConditionalComponent> conditionalComponents = canvas.components
                .Where(x => x is ConditionalComponent)
                .Cast<ConditionalComponent>();

            IEnumerable<ConnectionComponent> connectionComponents = canvas.components
                .Where(x => x is ConnectionComponent)
                .Cast<ConnectionComponent>();

            IEnumerable<Node> nodesWithoutComponent = target.nodes
                .Except(nodeComponents.Select(x => x.node))
                .OrderBy(x => x.priority);

            foreach (Node node in nodesWithoutComponent)
            {
                NodeComponent.NodeComponentCreateArgs args = new NodeComponent.NodeComponentCreateArgs();
                args.fromExisting = node;

                canvas.Create<NodeComponent>(args);
            }

            IEnumerable<NodeComponent> componentsWithoutNode = nodeComponents
                .Where(x => !target.nodes.Contains(x.node));

            foreach (NodeComponent nodeComponent in componentsWithoutNode)
                GUIComponent.Destroy(nodeComponent);

            IEnumerable<Conditional> conditionalsWithoutComponent = target.nodes
                .Select(x => x.conditionals)
                .SelectMany(x => x)
                .Except(conditionalComponents.Select(x => x.conditional));

            foreach (Conditional conditional in conditionalsWithoutComponent)
            {
                ConditionalComponent.ConditionalComponentCreateArgs args = new ConditionalComponent.ConditionalComponentCreateArgs();
                args.fromExisting = conditional;

                canvas.Create<ConditionalComponent>(args);
            }

            IEnumerable<ConditionalComponent> componentsWithoutConditional = conditionalComponents
                .Where(x => !target.nodes.Select(x => x.conditionals).SelectMany(x => x).Contains(x.conditional));

            foreach (ConditionalComponent conditionalComponent in componentsWithoutConditional)
                GUIComponent.Destroy(conditionalComponent);

            IEnumerable<Tuple<Node, Node>> connectionsWithoutComponent = target.nodes
                .Select(x => x.children.Select(y => new Tuple<Node, Node>(y.parent, y)))
                .SelectMany(x => x)
                .Except(connectionComponents.Select(x => new Tuple<Node, Node>(x.from.node, x.to.node)));

            foreach (Tuple<Node, Node> n in connectionsWithoutComponent)
            {
                NodeComponent parent = (NodeComponent)canvas.FindComponent(n.Item1);
                NodeComponent child = (NodeComponent)canvas.FindComponent(n.Item2);

                ConnectionComponent.ConnectionComponentCreateArgs args = new ConnectionComponent.ConnectionComponentCreateArgs();

                args.from = parent;
                args.to = child;

                child.parentConnection = canvas.Create<ConnectionComponent>(args);
            }

            IEnumerable<ConnectionComponent> componentsWithoutConnection = connectionComponents
                .Where(x => !target.nodes
                    .Select(x => x.children.Select(y => new Tuple<Node, Node>(y.parent, y)))
                    .SelectMany(x => x).Contains(new Tuple<Node, Node>(x.from.node, x.to.node))
                    );

            foreach (ConnectionComponent connectionComponent in componentsWithoutConnection)
                GUIComponent.Destroy(connectionComponent);
        }

        private void DoSplashCanvas()
        {
            if (canvas == null)
                canvas = new ComponentCanvas(this, null, null, null, DrawGrid);

            CalculateWindow();

            WindowComponent.WindowComponentCreateArgs windowCreateArgs
                = new WindowComponent.WindowComponentCreateArgs();

            float height = 512f;
            float width = 512f;

            windowCreateArgs.id = 1;
            windowCreateArgs.rect = new Rect((window.width - width) / 2f, (window.height - height) / 2f, width, height);
            windowCreateArgs.style = Styles.window;
            windowCreateArgs.title = new GUIContent("Open Graph");
            windowCreateArgs.windowProvider = new Splash();
            windowCreateArgs.canClose = false;

            canvas.Create<WindowComponent>(windowCreateArgs);
        }

        private void CreateAddNodeWindow()
        {
            QuickSearch search = new QuickSearch(
                HelperMethods.GetEnumerableOfType(typeof(Node)),
                t =>
                {
                    NodeComponent.NodeComponentCreateArgs nodeCreateArgs =
                        new NodeComponent.NodeComponentCreateArgs();
                    nodeCreateArgs.graph = target;
                    nodeCreateArgs.nodeType = t;
                    nodeCreateArgs.position = canvas.zoomer.WindowToGridPosition(window.center);

                    canvas.Create<NodeComponent>(nodeCreateArgs);
                }
            );

            WindowComponent.WindowComponentCreateArgs createArgs = new WindowComponent.WindowComponentCreateArgs();

            createArgs.id = 1;
            createArgs.layer = 100;
            createArgs.rect = new Rect((window.width - 500f) / 2f, (window.width - 500f) / 2f, 500f, 500f);
            createArgs.style = Styles.window;
            createArgs.title = GUIContent.none;
            createArgs.windowProvider = search;
            createArgs.canClose = true;

            canvas.Create<WindowComponent>(createArgs);
        }

        private bool CanAddConditional()
        {
            return CanAddConditional(canvas);
        }

        public static bool CanAddConditional(ComponentCanvas canvas)
        {
            return canvas.selected.Length == 0 ||
                        !canvas.selected.All(c =>
                            (c is NodeComponent
                             && (((NodeComponent)c).node.connectionDescriptor == Node.ConnectionDescriptor.Both
                                 || ((NodeComponent)c).node.connectionDescriptor == Node.ConnectionDescriptor.OnlyInConnection))
                            || c is ConnectionComponent
                        );
        }

        private void CreateAddConditionalWindow()
        {
            QuickSearch search = new QuickSearch(
                HelperMethods.GetEnumerableOfType(typeof(Conditional)),
                t =>
                {
                    foreach (GUIComponent component in canvas.selected)
                    {
                        ConditionalComponent.ConditionalComponentCreateArgs conditionalCreateArgs =
                            new ConditionalComponent.ConditionalComponentCreateArgs();
                        conditionalCreateArgs.node = component is NodeComponent
                            ? ((NodeComponent)component).node
                            : ((ConnectionComponent)component).to.node;
                        conditionalCreateArgs.conditionalType = t;

                        canvas.Create<ConditionalComponent>(conditionalCreateArgs);
                    }
                }
            );

            WindowComponent.WindowComponentCreateArgs createArgs = new WindowComponent.WindowComponentCreateArgs();

            createArgs.id = 1;
            createArgs.layer = 100;
            createArgs.rect = new Rect((window.width - 500f) / 2f, (window.width - 500f) / 2f, 500f, 500f);
            createArgs.style = Styles.window;
            createArgs.title = GUIContent.none;
            createArgs.windowProvider = search;
            createArgs.canClose = true;

            canvas.Create<WindowComponent>(createArgs);
        }

        /// <summary>
        ///     Draws the grid to the screen based on zoom and pan
        /// </summary>
        public void DrawGrid(Rect rect, float zoom, Vector2 panOffset)
        {
            if (!Prefs.showGrid)
            {
                EditorGUI.DrawRect(rect, Styles.windowAccent);
                return;
            }

            float transitionPoint = 2f;
            float transitionWindow = 0.25f;

            rect.position = Vector2.zero;

            Vector2 center = rect.size * .5f;
            Texture2D gridTex = zoom > 2f ? Icons.gridTexture2x : Icons.gridTexture;

            float fac = 1f - Mathf.Clamp(zoom - (transitionPoint - transitionWindow), 0f, transitionWindow * 2f) /
                (transitionWindow * 2f);

            // Offset from origin in tile units
            float xOffset = -(center.x * zoom + panOffset.x) / gridTex.width;
            float yOffset = ((center.y - rect.size.y) * zoom + panOffset.y) / gridTex.height;

            Vector2 tileOffset = new Vector2(xOffset, yOffset);

            // Amount of tiles
            float tileAmountX = Mathf.Round(rect.size.x * zoom) / gridTex.width;
            float tileAmountY = Mathf.Round(rect.size.y * zoom) / gridTex.height;

            Vector2 tileAmount = new Vector2(tileAmountX, tileAmountY);

            GUI.DrawTextureWithTexCoords(rect, Icons.gridTexture2x, new Rect(tileOffset, tileAmount));
            GUI.color = new Color(1f, 1f, 1f, fac);
            GUI.DrawTextureWithTexCoords(rect, Icons.gridTexture, new Rect(tileOffset, tileAmount));
            GUI.color = Color.white;
        }

        [Serializable]
        public class Window
        {
            public enum InspectorView
            {
                Inspector,
                Blackboard
            }

            public NodeEditor editor;
            public Vector2 inspectorScroll;
            public InspectorView inspectorView;
            public bool inspectorToggled = true;

            public static readonly float inspectorWidth = 350f;

            public static readonly float padding = 8f;
        }
    }
}