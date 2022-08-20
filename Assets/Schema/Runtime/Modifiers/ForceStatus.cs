namespace Schema.Builtin.Modifiers
{
    [AllowOne, DarkIcon("console.infoicon", true)]
    public class ForceStatus : Modifier
    {
        public enum ForcedStatus
        {
            Success,
            Failure
        }

        public ForcedStatus forcedStatus;

        public override Message Modify(object modifierMemory, SchemaAgent agent, NodeStatus status)
        {
            switch (forcedStatus)
            {
                case ForcedStatus.Success:
                    return Message.ForceSuccess;
                case ForcedStatus.Failure:
                    return Message.ForceFailure;
                default:
                    return Message.None;
            }
        }
    }
}