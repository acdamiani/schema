using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DownloadIcon))]
public class DownloadIconEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Click"))
        {
            GameObject gameObject = GameObject.Find("Player");
            Quaternion n = Quaternion.identity;
            DynamicProperty.Set(gameObject, "/transform/rotation", n);
        }
    }
}