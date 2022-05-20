using Schema;
using UnityEngine;

[DarkIcon("Dark/FindGameObjectWithTag")]
[LightIcon("Light/FindGameObjectWithTag")]
public class FindGameObjectWithTag : Action
{
    public TagList gameObjectTag;
    [WriteOnly]
    [DisableDynamicBinding]
    [Tooltip("The Blackboard Key in which to store the found object")]
    public BlackboardEntrySelector<GameObject> gameObject;
    public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
    {
        GameObject found = GameObject.FindGameObjectWithTag(gameObjectTag.tag);

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
