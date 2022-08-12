namespace Schema.Builtin.Nodes
{
    [Description("Will stop execution of the tree perpetually, until flow is pulled from it by a Decorator.")]
    public class Idle : Action
    {
        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            return NodeStatus.Running;
        }
    }
}