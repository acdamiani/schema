using Schema;
using SchemaEditor;
using SchemaEditor.Utilities;
using SchemaEditor.Internal;
using UnityEngine;
using UnityEditor;
using System;
using Schema.Utilities;

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

        if (canvas.selected.Length != 1 && pinned == null)
            return;

        GUIComponent current = pinned != null ? pinned : canvas.selected[0];

        string componentDebugInfo = current.GetDebugInfo();

        float height = label.CalcHeight(new GUIContent(current.GetDebugInfo()), 400f);

        rect = new Rect(this.rect.position.x + 10f, this.rect.position.y + 10f, 410f, height + 10f);

        GUI.Box(rect, GUIContent.none, GUI.skin.window);
        EditorGUI.LabelField(rect.Pad(5), new GUIContent(current.GetDebugInfo()), label);

        if (GUI.Toggle(new Rect(rect.xMax - 44f, rect.y + 2f, 42f, 32f), pinned != null, "Pin"))
            pinned = pinned ?? canvas.selected[0];
        else
            pinned = null;
    }
    public Rect GetRect() { return pinned != null || canvas.selected.Length == 1 ? rect : Rect.zero; }
}