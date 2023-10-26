using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("d_cs Script Icon", true), LightIcon("cs Script Icon", true), Category("MonoBehaviour"),
     Description("Stop all coroutines on a MonoBehavior")]
    public class StopAllCoroutines : Action
    {
        [Tooltip("MonoBehaviour with the running coroutine that you would like to terminate")]
        public BlackboardEntrySelector<MonoBehaviour> monoBehaviour;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            if (monoBehaviour.value == null)
                return NodeStatus.Failure;

            monoBehaviour.value.StopAllCoroutines();

            return NodeStatus.Success;
        }
    }
}