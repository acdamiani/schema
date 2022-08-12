using System;
using UnityEngine;

namespace SchemaEditor.Internal.ComponentSystem.Components
{
    public sealed class BlockerComponent : GUIComponent, ICanvasMouseEventSink
    {
        public Func<Rect> rect { get; set; }

        public Rect GetRect()
        {
            return rect();
        }

        public override void Create(CreateArgs args)
        {
            BlockerComponentCreateArgs createArgs = args as BlockerComponentCreateArgs;

            if (createArgs == null)
                throw new ArgumentException();

            rect = createArgs.rect;
        }

        public override void OnGUI()
        {
        }

        public class BlockerComponentCreateArgs : CreateArgs
        {
            public Func<Rect> rect { get; set; }
        }
    }
}