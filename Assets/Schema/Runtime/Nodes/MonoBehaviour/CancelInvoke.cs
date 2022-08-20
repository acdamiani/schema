using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("d_cs Script Icon", true), LightIcon("cs Script Icon", true), Category("MonoBehaviour"),
     Description("Cancel all invokes on a MonoBehavior, or an invoke for a specified method")]
    public class CancelInvoke : Action
    {
        [Tooltip("MonoBehaviour to stop methods on")]
        public BlackboardEntrySelector<MonoBehaviour> monoBehaviour;

        [Tooltip("Whether to cancel all methods")]
        public bool cancelAll;

        [Tooltip("Name of method to cancel invoke for")]
        public BlackboardEntrySelector<string> methodName;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            if (monoBehaviour.value == null)
                return NodeStatus.Failure;

            if (cancelAll)
                monoBehaviour.value.CancelInvoke();
            else
                monoBehaviour.value.CancelInvoke(methodName.value);

            return NodeStatus.Success;
        }
    }
}