using System;

namespace Schema
{
    /// <summary>
    ///     Base class for all Action nodes.
    ///     Extending this class will make your node appear in the Node Editor automatically.
    /// </summary>
    [Serializable]
    public abstract class Action : Node
    {
        public override ConnectionDescriptor connectionDescriptor => ConnectionDescriptor.OnlyInConnection;

        public override bool CanHaveChildren()
        {
            return false;
        }

        /// <summary>
        ///     Runs once when all nodes are first initialized. Similar to Start() in a MonoBehavior class
        /// </summary>
        /// <param name="nodeMemory">Object containing the memory for the node</param>
        /// <param name="agent">Agent executing this node</param>
        public virtual void OnInitialize(object nodeMemory, SchemaAgent agent)
        {
        }

        /// <summary>
        ///     Runs once when the node is first ticked
        /// </summary>
        /// <param name="nodeMemory">Object containing the memory for the node</param>
        /// <param name="agent">Agent executing this node</param>
        public virtual void OnNodeEnter(object nodeMemory, SchemaAgent agent)
        {
        }

        /// <summary>
        ///     Runs once when the node has returned a status of Success or Failure
        /// </summary>
        /// <param name="nodeMemory">Object containing the memory for the node</param>
        /// <param name="agent">Agent executing this node</param>
        public virtual void OnNodeExit(object nodeMemory, SchemaAgent agent)
        {
        }

        /// <summary>
        ///     Where the logic of the node occurs. Return a status of Success or Failure to continue the tree execution,
        ///     or return Running to tick the node again next frame
        /// </summary>
        /// <param name="nodeMemory">Object containing the memory for the node</param>
        /// <param name="agent">Agent executing this node</param>
        public abstract NodeStatus Tick(object nodeMemory, SchemaAgent agent);
    }
}