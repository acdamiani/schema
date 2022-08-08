using Schema;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("Nodes/d_Debug")]
    [LightIcon("UnityEditor.ConsoleWindow@2x", true)]
    [Category("Debug")]
    [Description("Logs a rich-text enabled message to the console")]
    public class DebugLog : Action
    {
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
        public enum LogType
        {
            Log,
            Warning,
            Error
        }
    }
}