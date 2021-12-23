using Schema.Runtime;
using UnityEngine;

[DarkIcon("Dark/FindGameObjectWithTag")]
[LightIcon("Light/FindGameObjectWithTag")]
public class FindGameObjectWithTag : Action
{
    class FindGameObjectWithTagMemory
    {
        public BlackboardData data;
    }
    public TagList gameObjectTag;
    [Tooltip("The Blackboard Key in which to store the found object")]
    public BlackboardGameObject gameObject;
    public override void OnInitialize(object nodeMemory, SchemaAgent agent)
    {
        FindGameObjectWithTagMemory memory = (FindGameObjectWithTagMemory)nodeMemory;
        memory.data = agent.GetBlackboardData();
    }
    public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
    {
        FindGameObjectWithTagMemory memory = (FindGameObjectWithTagMemory)nodeMemory;

        BlackboardData.EntryData entry = memory.data.GetEntry(gameObject.entryID);

        if (!string.IsNullOrEmpty(gameObject.entryID))
        {
            GameObject found = GameObject.FindGameObjectWithTag(gameObjectTag.tag);

            if (!found)
            {
                return NodeStatus.Failure;
            }
            else
            {
                memory.data.SetValue<GameObject>(gameObject.entryID, found);
                return NodeStatus.Success;
            }
        }
        else
        {
            return NodeStatus.Failure;
        }
    }
}
