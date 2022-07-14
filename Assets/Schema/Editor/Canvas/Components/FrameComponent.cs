using UnityEngine;
using UnityEditor;
using Schema;
using Schema.Utilities;
using SchemaEditor.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SchemaEditor.Internal.ComponentSystem.Components
{
    public class FrameComponent : GUIComponent, ISelectable, IEditable
    {
        private bool isSelected;
        private Rect frameRect;
        public Frame frame { get; set; }
        private IFramable[] framed;
        public override void Create(CreateArgs args)
        {
            FrameComponentCreateArgs createArgs = args as FrameComponentCreateArgs;

            if (createArgs == null)
                throw new ArgumentException();

            if (createArgs.frame == null)
                throw new ArgumentException("Frame to create the component from is null!");

            createArgs.layer = -1;

            frame = createArgs.frame;

            ResolveFramables(frame.children);
        }
        private void ResolveFramables(IEnumerable<UnityEngine.Object> objects)
        {
            IEnumerable<IFramable> framables = objects
                .Select(x => canvas.Resolve(x))
                .Where(x => x != null && x is IFramable)
                .Cast<IFramable>();

            framed = framables.ToArray();
        }
        public override void OnGUI()
        {
            float minX = framed.Min(x => x.xMin) - 50f;
            float minY = framed.Min(x => x.yMin) - 50f;
            float maxX = framed.Max(x => x.xMax) + 50f;
            float maxY = framed.Max(x => x.yMax) + 50f;

            frameRect = new Rect(minX, minY, maxX - minX, maxY - minY);

            GUI.color = frame.useCustomColor ? frame.customColor : new Color(0.05f, 0.05f, 0.05f, 1f);
            Styles.shadow.DrawIfRepaint(frameRect.Pad(-14), false, false, false, false);
            Styles.roundedBox.DrawIfRepaint(frameRect, false, false, false, false);
            GUI.color = Color.white;

            if (isSelected)
                Styles.styles.nodeSelected.DrawIfRepaint(frameRect, false, false, false, false);

            Styles.title.DrawIfRepaint(frameRect.Pad(4), new GUIContent("Hello"), false, false, false, false);
        }
        public void Select(bool additive) { isSelected = true; }
        public void Deselect() { isSelected = false; }
        public bool IsSelectable() { return true; }
        public bool IsHit(Vector2 mousePosition) { return frameRect.Contains(mousePosition); }
        public bool Overlaps(Rect rect) { return frameRect.Overlaps(rect); }
        public bool IsEditable() { return true; }
        public UnityEngine.Object GetEditable() { return frame; }
        public class FrameComponentCreateArgs : CreateArgs
        {
            public Schema.Frame frame { get; set; }
        }
    }
}