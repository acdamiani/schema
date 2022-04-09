using Schema.Runtime;

[Description("Where the execution of the tree begins")]
public class Root : Node
{
    public override bool _canHaveParent => false;
    public override int _maxChildren => 1;
}