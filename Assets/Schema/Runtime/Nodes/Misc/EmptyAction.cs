namespace Schema.Builtin.Nodes
{
    [Description("Immediately returns success"), Category("Miscellaneous")]
    public class EmptyAction : Action
    {
        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            return NodeStatus.Success;
        }
    }
}