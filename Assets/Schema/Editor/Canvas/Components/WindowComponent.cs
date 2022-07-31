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
    public sealed class WindowComponent : GUIComponent, ICanvasMouseEventSink
    {
        public override void Create(CreateArgs args)
        {
            WindowComponentCreateArgs createArgs = args as WindowComponentCreateArgs;

            if (createArgs == null)
                throw new ArgumentException();

            id = createArgs.id;
            _rect = createArgs.rect;
            windowProvider = createArgs.windowProvider;
            title = createArgs.title;
            style = createArgs.style;
        }
        public Rect rect { get { return _rect; } }
        private Rect _rect;
        public int id { get; set; }
        public GUIContent title { get; set; }
        public GUIStyle style { get; set; }
        private IWindowComponentProvider windowProvider { get; set; }
        public override void OnGUI()
        {
            GUI.FocusWindow(id);

            bool insideWindow = rect.Contains(Event.current.mousePosition);

            if (
                (Event.current.rawType == EventType.MouseDown && !insideWindow)
                || (Event.current.rawType == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
            )
            {
                Destroy(this);
                return;
            }

            EditorWindow e = canvas.context.GetEditorWindow();

            if (e != null)
                e.BeginWindows();

            windowProvider.HandleWinInfo(_rect, title, style);
            _rect = GUI.Window(id, _rect, windowProvider.OnGUI, title, style);

            if (e != null)
                e.EndWindows();
        }
        public Rect GetRect() { return rect; }
        public class WindowComponentCreateArgs : CreateArgs
        {
            public Rect rect { get; set; }
            public IWindowComponentProvider windowProvider { get; set; }
            public int id { get; set; }
            public GUIContent title { get; set; }
            public GUIStyle style { get; set; }
        }
    }
}