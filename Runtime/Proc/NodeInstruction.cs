using System;

namespace Schema.Internal
{
    public class NodeInstruction
    {
        [Flags]
        public enum Instruction
        {
            None = 0,
            Quit = 1,
            ForceStatus = 2,
            Repeat = 4
        }

        public bool active { get; set; }
        public NodeStatus statusOverride { get; set; }
        public Instruction instruction { get; private set; }

        public void Reset()
        {
            instruction = Instruction.None;
            active = false;
        }

        public void Instruct(params Instruction[] instructions)
        {
            active = true;

            for (int i = 0; i < instructions.Length; i++)
            {
                Instruction cur = instructions[i];

                if (!instruction.HasFlag(cur))
                    instruction |= cur;
            }
        }
    }
}