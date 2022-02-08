using Schema.Runtime;
using UnityEngine;

[DarkIcon("Dark/FindGameObjectWithTag")]
[LightIcon("Light/FindGameObjectWithTag")]
public class FindGameObjectWithTag : Action
{
    public TagList gameObjectTag;
    [Tooltip("The Blackboard Key in which to store the found object")]
    public BlackboardEntrySelector<GameObject> gameObject;
    public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
    {
        BlackboardData.EntryData entry = agent.blackboard.GetEntry(gameObject.entryID);

        if (!string.IsNullOrEmpty(gameObject.entryID))
        {
            GameObject found = GameObject.FindGameObjectWithTag(gameObjectTag.tag);

            if (!found)
            {
                return NodeStatus.Failure;
            }
            else
            {
                agent.blackboard.SetValue<GameObject>(gameObject.entryID, found);
                return NodeStatus.Success;
            }
        }
        else
        {
            return NodeStatus.Failure;
        }
    }
}
