using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("Nodes/d_Debug"), LightIcon("Nodes/Debug"), Category("Debug"),
     Description("Logs a formatted message to teh console")]
    public class DebugLogFormat : Action
    {
        [TextArea] public string message;
        public List<BlackboardEntrySelector> keys;

        private void OnValidate()
        {
            if (keys != null)
                foreach (BlackboardEntrySelector key in keys)
                    key.ApplyAllFilters();
        }

        protected override void OnObjectEnable()
        {
            if (keys != null)
                foreach (BlackboardEntrySelector key in keys)
                    key.ApplyAllFilters();
        }

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            object[] values = keys.Select(key => key.value).ToArray();

            try
            {
                Debug.LogFormat(message, values);
                return NodeStatus.Success;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return NodeStatus.Failure;
            }
        }
    }
}