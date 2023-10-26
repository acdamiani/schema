namespace Schema.Builtin.Nodes
{
    [DarkIcon("Nodes/d_Selector"), LightIcon("Light/Selector")]
    public class Selector : Flow
    {
        public override int Tick(object nodeMemory, NodeStatus status, int index)
        {
            if (index == -1 && children.Length > 0)
                return 0;

            if (index + 1 > children.Length - 1 || status == NodeStatus.Success)
                return -1;

            return index + 1;
        }
    }
}