namespace Schema.Builtin.Nodes
{
    [Category("Miscellaneous")]
    public class ReturnResult : Action
    {
        public NodeStatus returnsStatus;

        public override NodeStatus Tick(object NodeMemory, SchemaAgent agent)
        {
            return returnsStatus;
        }
    }
}