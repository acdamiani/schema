using Schema.Runtime;
using UnityEngine;

[DarkIcon("Dark/FindGameObject")]
[LightIcon("Light/FindGameObject")]
public class FindGameObject : Action
{
    class FindGameObjectMemory
    {
        public BlackboardData data;
    }
    [Tooltip("The Blackboard Key in which to store the found object")]
    public string gameObjectName;
    public BlackboardGameObject gameObject;
    public override void OnInitialize(object nodeMemory, SchemaAgent agent)
    {
        FindGameObjectMemory memory = (FindGameObjectMemory)nodeMemory;
        memory.data = agent.GetBlackboardData();
    }
    public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
    {
        FindGameObjectMemory memory = (FindGameObjectMemory)nodeMemory;

        BlackboardData.EntryData entry = memory.data.GetEntry(gameObject.entryID);

        if (!string.IsNullOrEmpty(gameObject.entryID))
        {
            GameObject found = GameObject.Find(gameObjectName);

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
