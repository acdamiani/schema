using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class ObjectEditor
{
    private static readonly Dictionary<Object, Editor> editors = new();

    public static void DoEditor(Object obj)
    {
        editors.TryGetValue(obj, out Editor editor);

        if (editor == null)
            editor = editors[obj] = Editor.CreateEditor(obj);

        editor.OnInspectorGUI();
    }
}