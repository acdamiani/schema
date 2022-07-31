using UnityEngine;
using UnityEditor;

public class TestWindow : EditorWindow
{
    // Add menu named "My Window" to the Window menu
    [MenuItem("Window/My Window")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        TestWindow window = (TestWindow)EditorWindow.GetWindow(typeof(TestWindow));
        window.Show();
    }

    void OnGUI()
    {
        EditorGUI.DrawRect(new Rect(0f, 0f, 100f, 100f), Color.red);

        GUILayout.BeginArea(new Rect(0f, 0f, 100f, 100f));
        GUILayout.BeginHorizontal();

        if (Event.current.rawType != EventType.Layout && Event.current.rawType != EventType.Repaint)
            Debug.Log(Event.current.rawType);

        GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }
}