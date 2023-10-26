using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("d_cs Script Icon", true), LightIcon("cs Script Icon", true), Category("MonoBehaviour"),
     Description("Invoke a method specified by a name, at a specified time from now")]
    public class Invoke : Action
    {
        [Tooltip("MonoBehaviour to invoke the method on")]
        public BlackboardEntrySelector<MonoBehaviour> monoBehavior;

        [Tooltip("Name of method to invoke")] public BlackboardEntrySelector<string> methodName;

        [Tooltip("Time from now to invoke the method")]
        public BlackboardEntrySelector<float> time;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            if (monoBehavior.value == null)
                return NodeStatus.Failure;

            monoBehavior.value.Invoke(methodName.value, time.value);

            return NodeStatus.Success;
        }
    }
}