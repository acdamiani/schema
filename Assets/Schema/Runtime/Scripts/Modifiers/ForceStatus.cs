namespace Schema.Builtin.Modifiers
{
    [DarkIcon("console.infoicon", true)]
    public class ForceStatus : Modifier
    {
        public ForcedStatus status;
        public override void OnNodeEnter(object modifierMemory, SchemaAgent agent)
        {
            if (status == ForcedStatus.Success)
                SendMessage(Message.ForceSuccess);
            else if (status == ForcedStatus.Success)
                SendMessage(Message.ForceFailure);
        }
        public enum ForcedStatus
        {
            Success,
            Failure
        }
    }
}