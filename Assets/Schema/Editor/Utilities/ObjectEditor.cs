using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Object = UnityEngine.Object;

public static class ObjectEditor
{
    private static Dictionary<Object, Editor> editors = new Dictionary<Object, Editor>();
    public static void DoEditor(Object obj)
    {
        editors.TryGetValue(obj, out Editor editor);

        if (editor == null)
            editor = editors[obj] = Editor.CreateEditor(obj);

        editor.OnInspectorGUI();
    }
}