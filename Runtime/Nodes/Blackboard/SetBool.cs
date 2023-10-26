namespace Schema.Builtin.Nodes
{
    [Category("Blackboard")]
    public class SetBool : Action
    {
        [WriteOnly] public BlackboardEntrySelector<bool> selector;
        public BlackboardEntrySelector<bool> value;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            selector.value = value.value;

            return NodeStatus.Success;
        }
    }
}