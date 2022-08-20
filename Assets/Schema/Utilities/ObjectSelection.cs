// I hate that I have to do this

#if UNITY_EDITOR
namespace Schema.Internal
{
    public static class ObjectSelection
    {
        public static Node[] nodeSelection = new Node[0];
        public static Conditional[] conditionalSelection = new Conditional[0];
    }
}
#endif