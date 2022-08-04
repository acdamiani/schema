using UnityEngine;
using UnityEditor;
using System;
using System.Text;
using Schema.Utilities;
using System.Linq;

namespace SchemaEditor.Internal.ComponentSystem.Components
{
    public sealed class DebugViewComponent : GUIComponent, ICanvasMouseEventSink
    {
        public GUIComponent pinned;
        public override void Create(CreateArgs args) { }
        private Rect rect;
        public override void OnGUI()
        {
            rect = canvas.context.GetRect();

            GUIStyle label = new GUIStyle(EditorStyles.label);
            label.fontSize = 10;
            label.wordWrap = true;
            label.richText = true;

            GUIComponent current = null;

            if (pinned != null)
                current = pinned;
            else if (canvas.selected.Length == 1)
                current = canvas.selected[0];

            string componentDebugInfo = current != null ? current.GetDebugInfo() : "";

            componentDebugInfo = DoInternalDebug() + componentDebugInfo;

            float height = label.CalcHeight(new GUIContent(componentDebugInfo), 400f);

            rect = new Rect(this.rect.position.x + 10f, this.rect.position.y + 10f, 410f, height + 10f);

            GUI.Box(rect, GUIContent.none, GUI.skin.window);
            EditorGUI.LabelField(rect.Pad(5), new GUIContent(componentDebugInfo), label);

            if (GUI.Toggle(new Rect(rect.xMax - 44f, rect.y + 2f, 42f, 32f), pinned != null, "Pin"))
                pinned = pinned ?? canvas.selected.ElementAtOrDefault(0);
            else
                pinned = null;
        }
        private string DoInternalDebug()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(String.Format("<b>Hovered:</b> {0}", canvas.hovered));
            sb.AppendLine(String.Format("<b>Zoom:</b> {0}", canvas.zoomer.zoom));
            sb.AppendLine(String.Format("<b>Pan:</b> {0}", canvas.zoomer.pan));
            sb.AppendLine(String.Format("<b>Components:</b> {0}", String.Join(", ", canvas.components.Select(x => x.GetType().Name))));
            sb.AppendLine();

            return sb.ToString();
        }
        public Rect GetRect() { return pinned != null || canvas.selected.Length == 1 ? rect : Rect.zero; }
    }
}