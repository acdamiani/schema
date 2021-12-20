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

        if (gameObject.entry != null)
        {
            GameObject found = GameObject.Find(gameObjectName);
            if (!found)
            {
                return NodeStatus.Failure;
            }
            else
            {
                memory.data.SetValue<GameObject>(gameObject.entry.Name, found);
                return NodeStatus.Success;
            }
        }
        else
        {
            return NodeStatus.Failure;
        }
    }
}
