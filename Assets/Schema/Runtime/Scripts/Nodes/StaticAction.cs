using System;
using UnityEngine;
using UnityEngine.Events;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("Dark/CustomAction")]
    [LightIcon("Light/CustomAction")]
    public class StaticAction : Action
    {
        [SerializeField] private NodeAction customAction;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            customAction.Invoke(agent);
            return NodeStatus.Success;
        }

        [Serializable]
        private class NodeAction : UnityEvent<SchemaAgent>
        {
        }
    }
}