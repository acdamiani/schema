using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("Nodes/d_Debug"), LightIcon("Nodes/Debug"), Category("Debug"),
     Description("Logs a rich-text enabled message to the console")]
    public class DebugLog : Action
    {
        public enum LogType
        {
            Log,
            Warning,
            Error
        }

        [TextArea] public string message;
        public LogType logType;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            switch (logType)
            {
                case LogType.Log:
                    Debug.Log(message);
                    break;
                case LogType.Warning:
                    Debug.LogWarning(message);
                    break;
                case LogType.Error:
                    Debug.LogError(message);
                    break;
            }

            return NodeStatus.Success;
        }
    }
}