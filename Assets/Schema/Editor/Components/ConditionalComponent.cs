using Schema;
using SchemaEditor;
using SchemaEditor.Utilities;
using SchemaEditor.Internal;
using UnityEngine;
using UnityEditor;
using System;
using Schema.Utilities;

public sealed class ConditionalComponent : GUIComponent
{
    public override void Create(CreateArgs args)
    {
    }
    public override void OnGUI()
    {
    }
    public void Do(Vector2 pos)
    {
        float height = 32f;

        GUIContent content = new GUIContent("If var <color=red>$enemyPosition</color> is not null");

        Vector2 contentSize = Styles.conditional.CalcSize(content);

        Texture2D icon = Resources.Load<Texture2D>("Icon");
        float max = Mathf.Max(icon.width, icon.height);
        float vertContentHeight = height - Styles.conditional.padding.top - Styles.conditional.padding.bottom;
        Vector2 imageSize = new Vector2(icon.width / (max / vertContentHeight), icon.height / (max / vertContentHeight));

        Rect imageRect = new Rect(pos.x - 50f + Styles.conditional.padding.left, pos.y - 100f + (height - imageSize.y) / 2f, imageSize.x, imageSize.y);

        Rect contentRect = new Rect(pos.x - 50f, pos.y - 100f, contentSize.x + imageRect.width + 6f, height);

        EditorGUI.DrawRect(new Rect(contentRect.center.x, contentRect.y, 48f, 12f).UseCenter(), Styles.windowBackground);
        EditorGUI.DrawRect(new Rect(contentRect.center.x, contentRect.yMax, 48f, 12f).UseCenter(), Styles.windowBackground);

        GUI.backgroundColor = Styles.windowBackground;
        Styles.conditional.DrawIfRepaint(contentRect, content, false, false, false, false);
        GUI.backgroundColor = Styles.outlineColor;
        Styles.styles.nodeSelected.DrawIfRepaint(contentRect, false, false, false, false);
        GUI.DrawTexture(imageRect, icon);
        GUI.backgroundColor = Color.white;
    }
}