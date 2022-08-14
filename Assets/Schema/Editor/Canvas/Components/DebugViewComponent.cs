using System.Linq;
using System.Text;
using Schema.Utilities;
using UnityEditor;
using UnityEngine;

namespace SchemaEditor.Internal.ComponentSystem.Components
{
    public sealed class DebugViewComponent : GUIComponent, ICanvasMouseEventSink
    {
        public GUIComponent pinned;
        private Rect rect;

        public Rect GetRect()
        {
            return pinned != null || canvas.selected.Length == 1 ? rect : Rect.zero;
        }

        public override void Create(CreateArgs args)
        {
        }

        public override void OnGUI()
        {
            return;
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

            componentDebugInfo = DoInternalDebug(current) + componentDebugInfo;

            float height = label.CalcHeight(new GUIContent(componentDebugInfo), 400f);

            rect = new Rect(rect.position.x + 10f, rect.position.y + 10f, 410f, height + 10f);

            GUI.Box(rect, GUIContent.none, GUI.skin.window);
            EditorGUI.LabelField(rect.Pad(5), new GUIContent(componentDebugInfo), label);

            if (GUI.Button(new Rect(rect.xMax - 44f, rect.y + 2f, 42f, 32f), "T"))
                canvas.zoomer.pan = Vector2.zero;

            // if (GUI.Toggle(new Rect(rect.xMax - 44f, rect.y + 2f, 42f, 32f), pinned != null, "Pin"))
            //     pinned = pinned ?? canvas.selected.ElementAtOrDefault(0);
            // else
            //     pinned = null;
        }

        private string DoInternalDebug(GUIComponent current)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(string.Format("<b>Hovered:</b> {0}", canvas.hovered));
            sb.AppendLine(string.Format("<b>Zoom:</b> {0}", canvas.zoomer?.zoom));
            sb.AppendLine(string.Format("<b>Pan:</b> {0}", canvas.zoomer?.pan));
            sb.AppendLine(string.Format("<b>Components:</b> {0}",
                string.Join(", ", canvas.components.Select(x => x.GetType().Name))));
            sb.AppendLine();

            if (current != null) sb.AppendLine(string.Format("<b>Layer:</b> {0}", current.layer));

            return sb.ToString();
        }
    }
}