using System;
using UnityEngine;

namespace Schema.Builtin.Modifiers
{
    [AllowOne]
    [DisableIfTypes(typeof(LoopForever), typeof(LoopUntil))]
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
        public override Message Modify(object modifierMemory, SchemaAgent agent, NodeStatus status)
        {
            LoopMemory memory = (LoopMemory)modifierMemory;

            if (memory.count < loopCount)
            {
                memory.count++;
                return Message.Repeat;
            }

            memory.count = 0;
            return Message.None;
        }
        public enum ForcedStatus
        {
            Success,
            Failure
        }
    }
}