using UnityEditor;
using UnityEngine;
using System.Reflection;

[CustomEditor(typeof(DownloadIcon))]
public class DownloadIconEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Click"))
        {
            GameObject g = GameObject.Find("Player");

            Debug.Log(DynamicProperty.Get(g, "transform/position/x"));
        }
    }
}