using Schema;
using UnityEngine;

[DarkIcon("c_Schema.Graph")]
[LightIcon("c_Schema.Graph")]
public class FindGameObject : Action
{
    public string gameObjectName;
    [WriteOnly]
    [DisableDynamicBinding]
    [Tooltip("The Blackboard Key in which to store the found object")]
    public BlackboardEntrySelector<GameObject> gameObject;
    public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
    {
        GameObject found = GameObject.Find(gameObjectName);

        if (!found)
        {
            return NodeStatus.Failure;
        }
        else
        {
            gameObject.value = found;
            return NodeStatus.Success;
        }
    }
}
