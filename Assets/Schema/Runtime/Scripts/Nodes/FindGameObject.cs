using Schema;
using UnityEngine;

[DarkIcon("Dark/FindGameObject")]
[LightIcon("Light/FindGameObject")]
public class FindGameObject : Action
{
    public string gameObjectName;
    [WriteOnly]
    [DisableDynamicBinding]
    [Tooltip("The Blackboard Key in which to store the found object")]
    public BlackboardEntrySelector<GameObject> gameObject;
    public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
    {
        if (gameObject.empty)
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
        else
        {
            return NodeStatus.Failure;
        }
    }
}
