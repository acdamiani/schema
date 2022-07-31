using System;
using Schema;
using SchemaEditor;
using SchemaEditor.Utilities;
using SchemaEditor.Internal;
using UnityEngine;
using UnityEditor;
using Schema.Utilities;

namespace SchemaEditor.Internal.ComponentSystem.Components
{
    public sealed class BlockerComponent : GUIComponent, ICanvasMouseEventSink
    {
        public override void Create(CreateArgs args)
        {
            BlockerComponentCreateArgs createArgs = args as BlockerComponentCreateArgs;

            if (createArgs == null)
                throw new ArgumentException();

            rect = createArgs.rect;
        }
        public Func<Rect> rect { get; set; }
        public override void OnGUI() { }
        public Rect GetRect() { return rect(); }
        public class BlockerComponentCreateArgs : CreateArgs
        {
            public Func<Rect> rect { get; set; }
        }
    }
}