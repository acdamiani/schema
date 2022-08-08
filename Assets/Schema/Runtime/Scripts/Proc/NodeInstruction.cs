using System;

namespace Schema.Internal
{
    public class NodeInstruction
    {
        public bool active { get; set; }
        public NodeStatus statusOverride { get; set; }
        public Instruction instruction { get { return _instruction; } }
        private Instruction _instruction;
        public void Reset()
        {
            _instruction = Instruction.None;
            active = false;
        }
        public void Instruct(params Instruction[] instructions)
        {
            active = true;

            for (int i = 0; i < instructions.Length; i++)
            {
                Instruction cur = instructions[i];

                if (!_instruction.HasFlag(cur))
                    _instruction |= cur;
            }
        }
        [Flags]
        public enum Instruction
        {
            None = 0,
            Quit = 1,
            ForceStatus = 2,
            Repeat = 4
        }
    }
}