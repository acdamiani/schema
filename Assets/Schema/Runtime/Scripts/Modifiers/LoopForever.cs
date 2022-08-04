using System;
using UnityEngine;

namespace Schema.Builtin.Modifiers
{
    [AllowOne]
    [DarkIcon("Modifiers/LoopForever")]
    public class LoopForever : Modifier
    {
        public override void OnNodeExit(object modifierMemory, SchemaAgent agent, NodeStatus status)
        {
            SendMessage(Message.WaitAndRepeat);
        }
        public enum ForcedStatus
        {
            Success,
            Failure
        }
    }
}