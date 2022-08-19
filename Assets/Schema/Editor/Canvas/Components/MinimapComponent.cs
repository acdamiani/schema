using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SchemaEditor.Internal.ComponentSystem.Components
{
    public sealed class MinimapComponent : GUIComponent, ICanvasMouseEventSink
    {
        private Rect graphRect;
        private Rect gridViewRect;
        private Func<Vector2> offset;
        private Rect rect;
        private float viewWidth;

        public Rect GetRect()
        {
            return Prefs.minimapEnabled ? rect : Rect.zero;
        }

        public override void Create(CreateArgs args)
        {
            MinimapComponentCreateArgs createArgs = args as MinimapComponentCreateArgs;

            if (createArgs == null)
                throw new ArgumentException();

            offset = createArgs.offset;
        }

        public override void OnGUI()
        {
            if (canvas.selected.Length == 1)
            {
                GUIComponent selected = canvas.selected[0];

                string text = "";

                if (selected is NodeComponent)
                    text = ((NodeComponent)selected).node.description;
                else if (selected is ConditionalComponent)
                    text = ((ConditionalComponent)selected).conditional.description;

                if (!String.IsNullOrEmpty(text))
                    SchemaGUI.DoDescriptionLabel(canvas.context, text, Styles.description);
            }

            if (!Prefs.minimapEnabled)
                return;

            rect = canvas.context.GetViewRect();

            graphRect = GetGraphRect(100f);

            float height = graphRect.height / graphRect.width * Prefs.minimapWidth;

            viewWidth = height > Prefs.maxMinimapHeight
                ? Prefs.minimapWidth / height * Prefs.maxMinimapHeight
                : Prefs.minimapWidth;

            height = Mathf.Clamp(height, 0f, Prefs.maxMinimapHeight);

            float toolbarHeight = canvas.context.GetToolbarHeight();

            switch (Prefs.minimapPosition)
            {
                case 0:
                    rect = new Rect(10f, rect.yMax - height - 10f, Prefs.minimapWidth,
                        height);
                    break;
                case 1:
                    rect = new Rect(10f, 10f + toolbarHeight, Prefs.minimapWidth, height);
                    break;
                case 2:
                    rect = new Rect(rect.xMax - 10f - Prefs.minimapWidth, rect.yMax - height - 10f,
                        Prefs.minimapWidth, height);
                    break;
                case 3:
                    rect = new Rect(rect.xMax - 10f - Prefs.minimapWidth, toolbarHeight + 10f, Prefs.minimapWidth,
                        height);
                    break;
            }

            DoEvents();

            gridViewRect = canvas.zoomer.WindowToGridRect(canvas.context.GetViewRect());
            gridViewRect.position = GridToMinimapPosition(gridViewRect.position);
            gridViewRect.position = new Vector2(gridViewRect.position.x + rect.width / 2f - viewWidth / 2f,
                gridViewRect.position.y);
            gridViewRect.size = gridViewRect.size / graphRect.width * viewWidth;

            Handles.DrawSolidRectangleWithOutline(rect, new Color(0f, 0f, 0f, Prefs.minimapOpacity),
                Prefs.minimapOutlineColor);

            GUI.BeginGroup(rect);

            IEnumerable<NodeComponent> nodeComponents = canvas.components
                .Where(x => x is NodeComponent)
                .Cast<NodeComponent>()
                .Reverse();

            foreach (NodeComponent node in nodeComponents)
            {
                Vector2 position = GridToMinimapPosition(node.node.graphPosition);
                position.x += rect.width / 2f - viewWidth / 2f;

                Vector2 size = node.layout.body.size / graphRect.width * viewWidth;

                Rect nodeRect = new(position, size);

                Handles.DrawSolidRectangleWithOutline(nodeRect, Styles.windowBackground,
                    node.IsSelected() ? Prefs.selectionColor : Color.black);
            }


            Handles.DrawSolidRectangleWithOutline(gridViewRect, new Color(0f, 0f, 0f, 0f), Color.gray);

            GUI.EndGroup();
        }

        private void DoEvents()
        {
            Event e = Event.current;

            if (!rect.Contains(e.mousePosition))
                return;

            switch (e.rawType)
            {
                case EventType.MouseDrag:
                    Vector2 ratio = canvas.context.GetViewRect().size / gridViewRect.size;
                    canvas.zoomer.pan -= e.delta * ratio * canvas.zoomer.zoom;
                    break;
            }
        }

        public Vector2 GridToMinimapPosition(Vector2 position)
        {
            position = (position - graphRect.position) / graphRect.size;
            Vector2 sizeFac = new(viewWidth, graphRect.height / graphRect.width * viewWidth);

            return position * sizeFac;
        }

        public Vector2 MinimapToGridPosition(Vector2 position)
        {
            Vector2 sizeFac = new(viewWidth, graphRect.height / graphRect.width * viewWidth);
            position = position / sizeFac;

            position *= graphRect.size;
            position += graphRect.position;

            return position;
        }

        private Rect GetGraphRect(float padding)
        {
            IEnumerable<NodeComponent> nodeComponents = canvas.components
                .Where(x => x is NodeComponent)
                .Cast<NodeComponent>();

            if (nodeComponents.Count() == 0)
                return Rect.zero;

            float xMin = nodeComponents.Min(x => x.layout.gridRect.x) - padding;
            float xMax = nodeComponents.Max(x => x.layout.gridRect.x + x.layout.gridRect.width) + padding;
            float yMin = nodeComponents.Min(x => x.layout.gridRect.y) - padding;
            float yMax = nodeComponents.Max(x => x.layout.gridRect.y + x.layout.gridRect.height) + padding;

            graphRect = Rect.MinMaxRect(xMin, yMin, xMax, yMax);

            return graphRect;
        }

        public class MinimapComponentCreateArgs : CreateArgs
        {
            public Func<Vector2> offset { get; set; }
        }
    }
}