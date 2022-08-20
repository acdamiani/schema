using System;
using UnityEngine;
using UnityEngine.Events;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("Nodes/d_Action"), LightIcon("Nodes/Action"), Category("Miscellaneous")]
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