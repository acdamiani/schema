namespace Schema
{
    [Description("Where the execution of the tree begins")]
    public sealed class Root : Node
    {
        public override ConnectionDescriptor connectionDescriptor => ConnectionDescriptor.OnlyOutConnection;

        public override bool CanHaveParent()
        {
            return false;
        }

        public override bool CanHaveChildren()
        {
            return children.Length == 0;
        }
    }
}