using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [Description("Get the gameObject executing this node, and store it in a variable"), Category("Miscellaneous")]
    public class GetSelf : Action
    {
        [Tooltip("Where to store the gameObject executing this tree"), WriteOnly] 
        public BlackboardEntrySelector<GameObject> gameObject;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            if (gameObject.entry == null)
                return NodeStatus.Failure;

            gameObject.value = agent.gameObject;
            return NodeStatus.Success;
        }
    }
}