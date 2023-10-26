using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("d_GameObject Icon", true), LightIcon("GameObject Icon", true),
     Description("Find a game object by a given name"), Category("GameObject")]
    public class FindGameObject : Action
    {
        public string gameObjectName;

        [WriteOnly, DisableDynamicBinding, Tooltip("The Blackboard Key in which to store the found object")]  
        public BlackboardEntrySelector<GameObject> gameObject;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            GameObject found = GameObject.Find(gameObjectName);

            if (!found) return NodeStatus.Failure;

            gameObject.value = found;
            return NodeStatus.Success;
        }
    }
}