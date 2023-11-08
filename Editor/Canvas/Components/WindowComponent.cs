using System;
using UnityEditor;
using UnityEngine;

namespace SchemaEditor.Internal.ComponentSystem.Components
{
    public sealed class WindowComponent : GUIComponent, ICanvasMouseEventSink
    {
        public Rect rect { get; private set; }

        public int id { get; set; }
        public GUIContent title { get; set; }
        public GUIStyle style { get; set; }
        private IWindowComponentProvider windowProvider { get; set; }
        public bool canClose { get; set; }
        private bool wantsDestroy;

        public Rect GetRect()
        {
            return rect;
        }

        public override void Create(CreateArgs args)
        {
            WindowComponentCreateArgs createArgs = args as WindowComponentCreateArgs;

            if (createArgs == null)
                throw new ArgumentException();

            id = createArgs.id;
            rect = createArgs.rect;
            windowProvider = createArgs.windowProvider;
            title = createArgs.title;
            style = createArgs.style;
            canClose = createArgs.canClose;
            
            windowProvider.OnEnable();
        }

        public override void OnGUI()
        {
            bool insideWindow = rect.Contains(Event.current.mousePosition);
            bool shouldDestroy = false;

            if (
                canClose &&
                ((Event.current.rawType == EventType.MouseDown && !insideWindow)
                 || (Event.current.rawType == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
                 || windowProvider.ShouldClose())
            )
            {
                windowProvider.OnDestroy();
                wantsDestroy = true;
            }

            EditorWindow e = canvas.context.GetEditorWindow();

            if (!e) return;
            
            e.BeginWindows();

            windowProvider.HandleWinInfo(rect, title, style);
            rect = GUI.Window(id, rect, windowProvider.OnGUI, title, style);

            e.EndWindows();
            
            // I don't know why I have to wait until repaint, but I do.
            if (wantsDestroy && Event.current.type == EventType.Repaint)
                Destroy(this);
        }

        public class WindowComponentCreateArgs : CreateArgs
        {
            public Rect rect { get; set; }
            public IWindowComponentProvider windowProvider { get; set; }
            public int id { get; set; }
            public GUIContent title { get; set; }
            public GUIStyle style { get; set; }
            public bool canClose { get; set; }
        }
    }
}