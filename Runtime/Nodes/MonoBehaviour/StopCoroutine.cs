using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("d_cs Script Icon", true), LightIcon("cs Script Icon", true), Category("MonoBehaviour"),
     Description("Stop a named coroutine on a MonoBehavior")]
    public class StopCoroutine : Action
    {
        [Tooltip("MonoBehaviour with the running coroutine that you would like to terminate")]
        public BlackboardEntrySelector<MonoBehaviour> monoBehaviour;

        [Tooltip("Name of the coroutine to stop")]
        public BlackboardEntrySelector<string> methodName;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            if (monoBehaviour.value == null)
                return NodeStatus.Failure;

            monoBehaviour.value.StopCoroutine(methodName.value);

            return NodeStatus.Success;
        }
    }
}