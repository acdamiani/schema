using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("d_GameObject Icon", true), LightIcon("GameObject Icon", true), Category("GameObject"),
     Description("Find a game object by a given tag")]
    public class FindGameObjectWithTag : Action
    {
        public TagList gameObjectTag;

        [Tooltip("The Blackboard Key in which to store the found object"), WriteOnly] 
        public BlackboardEntrySelector<GameObject> gameObject;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            GameObject found = GameObject.FindGameObjectWithTag(gameObjectTag.tag);

            if (!found) return NodeStatus.Failure;

            gameObject.value = found;
            return NodeStatus.Success;
        }
    }
}