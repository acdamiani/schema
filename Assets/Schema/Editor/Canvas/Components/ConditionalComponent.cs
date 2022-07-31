using Schema;
using SchemaEditor;
using SchemaEditor.Utilities;
using SchemaEditor.Internal;
using UnityEngine;
using UnityEditor;
using System;
using Schema.Utilities;

namespace SchemaEditor.Internal.ComponentSystem.Components
{
    public sealed class ConditionalComponent : GUIComponent, IViewElement, ISelectable, IEditable
    {
        public Rect rect { get { return _rect; } }
        private Rect _rect;
        private NodeComponent parent;
        public Conditional conditional { get { return _conditional; } }
        private Conditional _conditional;
        public string uID { get { return _uID; } }
        private string _uID;
        public bool isSelected { get { return _isSelected; } }
        private bool _isSelected;
        public override void Create(CreateArgs args)
        {
            ConditionalComponentCreateArgs createArgs = args as ConditionalComponentCreateArgs;

            if (createArgs == null)
                throw new ArgumentException();

            if (createArgs.fromExisting != null)
            {
                _conditional = createArgs.fromExisting;
                _uID = _conditional.uID;
            }
            else
            {
                _conditional = createArgs.node.AddConditional(createArgs.conditionalType);
            }

            parent = (NodeComponent)canvas.FindComponent(_conditional.node);
        }
        public override void OnGUI()
        {
            if (parent == null)
                return;

            float height = 32f;

            Vector2 pos = new Vector2(parent.layout.body.center.x, parent.layout.body.y - height - 2f);

            GUIContent content = conditional.GetConditionalContent();
            Vector2 contentSize = Styles.conditional.CalcSize(content);

            Texture2D icon = conditional.icon;

            float iconWidth = icon?.width ?? 0f;
            float iconHeight = icon?.height ?? 0f;

            float max = Mathf.Max(iconWidth, iconHeight);

            float vertContentHeight = height - Styles.conditional.padding.top - Styles.conditional.padding.bottom;

            Vector2 imageSize = new Vector2(iconWidth / (max / vertContentHeight), iconHeight / (max / vertContentHeight));

            _rect = new Rect(pos.x, pos.y, contentSize.x + imageSize.y + 6f, height).UseCenter();
            Rect imageRect = new Rect(_rect.x + Styles.conditional.padding.left, _rect.y + (height - imageSize.y) / 2f, imageSize.x, imageSize.y);

            GUI.color = Styles.windowBackground;

            GUI.DrawTexture(new Rect(_rect.center.x, _rect.yMax - 10f, 56f, 54f).UseCenter(), Styles.Icons.GetResource("decorator_out", false));
            GUI.DrawTextureWithTexCoords(
                new Rect(_rect.center.x, parent.layout.body.y, 24f, 24f).UseCenter().Slice(0.5f, false, true),
                Styles.Icons.GetResource("in_connection", false),
                new Rect(0f, 0.5f, 1f, 0.5f)
            );

            GUI.color = Color.white;

            GUI.backgroundColor = Styles.windowBackground;
            Styles.element.DrawIfRepaint(_rect.Pad(-10), false, false, false, false);
            Styles.conditional.DrawIfRepaint(_rect, content, false, false, false, false);
            GUI.backgroundColor = isSelected ? NodeEditor.Prefs.selectionColor : Styles.outlineColor;
            Styles.styles.nodeSelected.DrawIfRepaint(_rect, false, false, false, false);

            if (icon != null)
                GUI.DrawTexture(imageRect, icon);

            GUI.backgroundColor = Color.white;
        }
        public bool IsSelectable() { return true; }
        public bool IsSelected() { return isSelected; }
        public bool IsHit(Vector2 position) { return rect.Contains(position); }
        public bool Overlaps(Rect rect) { return this.rect.Overlaps(rect, true); }
        public void Select(bool additive) { _isSelected = true; }
        public void Deselect() { _isSelected = false; }
        public UnityEngine.Object GetEditable() { return conditional; }
        public bool IsEditable() { return true; }
        public class ConditionalComponentCreateArgs : CreateArgs
        {
            public Node node { get; set; }
            public Conditional fromExisting { get; set; }
            public Type conditionalType { get; set; }
        }
    }
}