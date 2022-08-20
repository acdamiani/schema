using UnityEngine;

namespace Schema.Builtin.Modifiers
{
    [AllowOne, DisableIfTypes(typeof(LoopForever), typeof(LoopUntil)), DarkIcon("Modifiers/Loop")]
    public class Loop : Modifier
    {
        public enum ForcedStatus
        {
            Success,
            Failure
        }

        [Min(1)] public int loopCount;

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

        private class LoopMemory
        {
            public int count;
        }
    }
}