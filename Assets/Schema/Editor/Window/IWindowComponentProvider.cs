using UnityEngine;
using UnityEditor;

namespace SchemaEditor.Internal.ComponentSystem
{
    public interface IWindowComponentProvider
    {
        void HandleWinInfo(Rect rect, GUIContent title, GUIStyle style);
        void OnGUI(int id);
    }
}