using Schema.Internal;
using UnityEditor;

namespace SchemaEditor.Editors
{
    [CustomEditor(typeof(Blackboard))]
    public class BlackboardInspectorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUI.BeginDisabledGroup(true);
            base.OnInspectorGUI();
            EditorGUI.EndDisabledGroup();
        }
    }
}