namespace Schema.Builtin.Nodes
{
    [DarkIcon("Nodes/d_Sequence"), LightIcon("Nodes/Sequence"),
     Description("Executes a series of nodes one after another, disregarding node status")]
    public class Series : Flow
    {
        public override int Tick(object nodeMemory, NodeStatus status, int index)
        {
            if (index == -1 && children.Length > 0)
                return 0;

            if (index + 1 > children.Length - 1)
                return -1;

            return index + 1;
        }
    }
}