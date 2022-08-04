using System;
using UnityEngine;

namespace Schema.Builtin.Modifiers
{
    [AllowOne]
    [DarkIcon("Modifiers/Loop")]
    public class Loop : Modifier
    {
        class LoopMemory
        {
            public int count;
        }
        public int loopCount;
        void OnValidate()
        {
            loopCount = Mathf.Clamp(loopCount, 1, Int32.MaxValue);
        }
        public override void OnNodeEnter(object modifierMemory, SchemaAgent agent)
        {
            ((LoopMemory)modifierMemory).count = 0;
        }
        public override void OnNodeExit(object modifierMemory, SchemaAgent agent, NodeStatus status)
        {
            LoopMemory memory = (LoopMemory)modifierMemory;

            if (memory.count < loopCount)
            {
                SendMessage(Message.WaitAndRepeat);
                memory.count++;
            }
        }
        public enum ForcedStatus
        {
            Success,
            Failure
        }
    }
}