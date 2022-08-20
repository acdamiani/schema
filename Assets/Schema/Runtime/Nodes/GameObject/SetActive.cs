using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("d_GameObject Icon", true), LightIcon("GameObject Icon", true), Category("GameObject"),
     Description("Set a GameObject's active state based on a boolean value")]
    public class SetActive : Action
    {
        [Tooltip("GameObject to set active state")]
        public BlackboardEntrySelector<GameObject> gameObject;

        [Tooltip("Should this gameObject be active or not?")]
        public BlackboardEntrySelector<bool> active;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            if (gameObject.value == null)
                return NodeStatus.Failure;

            gameObject.value.SetActive(active.value);

            return NodeStatus.Success;
        }
    }
}