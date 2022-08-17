namespace Schema
{
    public abstract class Flow : Node
    {
        /// <summary>
        ///     Runs once when all nodes are first initialized. Similar to Start() in a MonoBehavior class
        /// </summary>
        /// <param name="nodeMemory">Object containing the memory for the node</param>
        /// <param name="agent">Agent executing this node</param>
        public virtual void OnInitialize(object flowMemory, SchemaAgent agent)
        {
        }

        /// <summary>
        ///     Runs when the execution enters the flow node
        /// </summary>
        /// <param name="nodeMemory">Object containing the memory for the node</param>
        /// <param name="agent">Agent executing this node</param>
        public virtual void OnFlowEnter(object flowMemory, SchemaAgent agent)
        {
        }

        /// <summary>
        ///     Runs when the execution exits the flow node
        /// </summary>
        /// <param name="nodeMemory">Object containing the memory for the node</param>
        /// <param name="agent">Agent executing this node</param>
        public virtual void OnFlowExit(object flowMemory, SchemaAgent agent)
        {
        }

        /// <summary>
        ///     Where the logic of the node occurs. Return the index of the child that you would like to execute next.
        ///     Alternatively, return -1 to move to the parent node
        /// </summary>
        /// <param name="nodeMemory">Object containing the memory for the node</param>
        /// <param name="agent">Agent executing this node</param>
        public abstract int Tick(object flowMemory, NodeStatus status, int index);
    }
}