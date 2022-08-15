using System;
using Schema;
using Schema.Internal;
using Schema.Utilities;
using SchemaEditor.Utilities;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using Object = UnityEngine.Object;

namespace SchemaEditor.Internal.ComponentSystem.Components
{
    public sealed class ConditionalComponent : GUIComponent, IViewElement, ISelectable, IEditable, IGraphObjectProvider,
        IDeletable, ICopyable
    {
        private static ConditionalComponent moving;
        private static Vector2 dxdyMouseDown;
        private static int desiredIndex;
        private static int originalIndex;
        private Rect _rect;
        private NodeComponent parent;
        public Rect rect => _rect;
        public Conditional conditional { get; private set; }

        public string uID { get; private set; }

        public bool isSelected { get; private set; }

        public bool IsDeletable()
        {
            IEnumerable<NodeComponent> nodeComponents = canvas.selected
                .Where(x => x is NodeComponent)
                .Cast<NodeComponent>();

            return isSelected && !nodeComponents.Contains(parent);
        }

        public void Delete()
        {
            conditional.node.RemoveConditional(conditional);
        }

        public Object GetEditable()
        {
            return conditional;
        }

        public bool IsEditable()
        {
            return true;
        }

        public bool Equals(GraphObject graphObject)
        {
            Conditional conditional = graphObject as Conditional;

            if (conditional != null)
                return conditional.uID == uID;
            return false;
        }

        public bool IsSelectable()
        {
            return true;
        }

        public bool IsSelected()
        {
            return isSelected;
        }

        public bool IsHit(Vector2 position)
        {
            return rect.Contains(position);
        }

        public bool Overlaps(Rect rect)
        {
            return this.rect.Overlaps(rect, true);
        }

        public void Select(bool additive)
        {
            isSelected = true;
            dxdyMouseDown = canvas.mousePositionNoZoom - rect.position;
            originalIndex = Array.IndexOf(conditional.node.conditionals, conditional);
        }

        public void Deselect()
        {
            isSelected = false;
        }

        public bool IsCopyable()
        {
            return isSelected;
        }

        public Object GetCopyable()
        {
            return conditional;
        }

        public Rect GetRect()
        {
            int index = Array.IndexOf(conditional.node.conditionals, conditional);
            int length = conditional.node.conditionals.Length;

            float height = 32f;

            int upCount = length - index;

            GUIContent content = conditional.GetConditionalContent();
            Texture2D icon = conditional.icon;

            Vector2 contentSize = Styles.conditional.CalcSize(content);
            contentSize.x += icon != null ? 20f : 0f;

            Vector2 pos = new Vector2(parent.layout.body.center.x - contentSize.x / 2f,
                parent.layout.body.y - (height + 18f) * upCount);

            return new Rect(pos.x, pos.y, contentSize.x, height);
        }

        public override void Create(CreateArgs args)
        {
            ConditionalComponentCreateArgs createArgs = args as ConditionalComponentCreateArgs;

            if (createArgs == null)
                throw new ArgumentException();

            createArgs.layer = 2;

            if (createArgs.fromExisting != null)
            {
                conditional = createArgs.fromExisting;
                uID = conditional.uID;
            }
            else
            {
                conditional = createArgs.node.AddConditional(createArgs.conditionalType);
                uID = conditional.uID;
            }

            parent = (NodeComponent)canvas.FindComponent(conditional.node);
        }

        public override void OnGUI()
        {
            if (parent == null)
                return;

            int index = Array.IndexOf(conditional.node.conditionals, conditional);
            int length = conditional.node.conditionals.Length;

            if (moving == this)
            {
                float dy = parent.layout.body.y - Event.current.mousePosition.y;
                desiredIndex = length - Mathf.RoundToInt(dy / 50f);
                index = desiredIndex;
            }
            else if (moving != null)
            {
                if (index > originalIndex && index <= desiredIndex)
                    index--;
                else if (index < originalIndex && index >= desiredIndex)
                    index++;
            }

            index = Mathf.Clamp(index, 0, length - 1);

            DoEvents();

            float height = 32f;

            int upCount = length - index;

            GUIContent content = conditional.GetConditionalContent();
            Texture2D icon = conditional.icon;

            Vector2 contentSize = Styles.conditional.CalcSize(content);
            contentSize.x += icon != null ? 20f : 0f;

            Vector2 pos = new Vector2(parent.layout.body.center.x - contentSize.x / 2f,
                parent.layout.body.y - (height + 18f) * upCount);

            _rect = new Rect(pos.x, pos.y, contentSize.x, height);

            GUI.color = Styles.windowBackground;

            Rect decorator_out = new Rect(_rect.center.x - 24f, _rect.yMax, 48f, 18f);

            GUI.DrawTexture(decorator_out, Icons.GetResource("decorator_out", false));
            GUI.DrawTextureWithTexCoords(
                new Rect(_rect.center.x - 12f, decorator_out.yMax - 12f, 24f, 12f),
                Icons.GetResource("in_connection", false),
                new Rect(0f, 0.5f, 1f, 0.5f)
            );

            GUI.color = Color.white;

            Rect r = _rect;

            if (moving == this)
            {
                GUI.backgroundColor = new Color(Styles.windowBackground.r, Styles.windowBackground.g,
                    Styles.windowBackground.b, 0.5f);
                Styles.element.DrawIfRepaint(_rect.Pad(-10), false, false, false, false);
                GUI.backgroundColor = new Color(1f, 1f, 1f, 0.25f);
                Styles.outline.DrawIfRepaint(_rect, false, false, false, false);

                r = new Rect(Event.current.mousePosition - dxdyMouseDown, new Vector2(contentSize.x, height));
            }

            GUI.backgroundColor = Styles.windowBackground;
            Styles.element.DrawIfRepaint(r.Pad(-10), false, false, false, false);
            Styles.conditional.DrawIfRepaint(r, content, false, false, false, false);
            GUI.backgroundColor = isSelected ? Prefs.selectionColor : Styles.outlineColor;
            Styles.outline.DrawIfRepaint(r, false, false, false, false);

            if (icon != null)
                GUI.DrawTexture(new Rect(r.x + Styles.conditional.padding.left, r.y + 8f, 16f, 16f), icon);

            GUI.backgroundColor = Color.white;
        }

        private void DoEvents()
        {
            Event e = Event.current;

            switch (e.rawType)
            {
                case EventType.MouseDrag when e.button == 0 && canvas.selected.Length == 1 && isSelected &&
                                              canvas.selectionBoxComponent.hidden:
                    moving = this;

                    break;
                case EventType.MouseUp when e.button == 0:
                    if (moving != null)
                        moving.conditional.node.MoveConditional(moving.conditional,
                             Mathf.Clamp(desiredIndex, 0, moving.conditional.node.conditionals.Length - 1));

                    moving = null;
                    break;
            }
        }

        public class ConditionalComponentCreateArgs : CreateArgs
        {
            public Node node { get; set; }
            public Conditional fromExisting { get; set; }
            public Type conditionalType { get; set; }
        }
    }
}