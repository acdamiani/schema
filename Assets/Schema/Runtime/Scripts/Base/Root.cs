using Schema;

namespace Schema
{
    [Description("Where the execution of the tree begins")]
    public sealed class Root : Node
    {
        public override bool canHaveParent => false;
        public override int maxChildren => 1;
    }
}