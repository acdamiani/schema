using Schema;

[Description("Immediately returns success")]
public class EmptyAction : Action
{
    public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
    {
        return NodeStatus.Success;
    }
}