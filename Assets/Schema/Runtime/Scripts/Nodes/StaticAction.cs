using UnityEngine;
using UnityEngine.Events;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("Dark/CustomAction")]
    [LightIcon("Light/CustomAction")]
    public class StaticAction : Action
    {
        [System.Serializable]
        private class NodeAction : UnityEvent<SchemaAgent> { }
        [SerializeField] private NodeAction customAction;
        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            customAction.Invoke(agent);
            return NodeStatus.Success;
        }
    }
}