using Schema;

namespace Schema
{
    [Description("Where the execution of the tree begins")]
    public sealed class Root : Node
    {
        public override bool CanHaveParent()
        {
            return false;
        }
        public override bool CanHaveChildren()
        {
            return base.CanHaveChildren();
        }
        public override int maxChildren => 1;
    }
}