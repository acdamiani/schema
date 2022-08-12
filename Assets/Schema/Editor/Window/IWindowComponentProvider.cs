using UnityEngine;

namespace SchemaEditor.Internal.ComponentSystem
{
    public interface IWindowComponentProvider
    {
        void HandleWinInfo(Rect rect, GUIContent title, GUIStyle style);
        void OnGUI(int id);
        bool ShouldClose();
    }
}