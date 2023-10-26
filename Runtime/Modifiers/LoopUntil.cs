namespace Schema.Builtin.Modifiers
{
    [AllowOne, DisableIfTypes(typeof(LoopForever), typeof(Loop)), DarkIcon("Modifiers/LoopUntil")]
    public class LoopUntil : Modifier
    {
        public enum ForcedStatus
        {
            Success,
            Failure
        }

        public BlackboardEntrySelector<bool> condition;

        public override Message Modify(object modifierMemory, SchemaAgent agent, NodeStatus status)
        {
            if (!condition.value)
                return Message.Repeat;

            return Message.None;
        }
    }
}