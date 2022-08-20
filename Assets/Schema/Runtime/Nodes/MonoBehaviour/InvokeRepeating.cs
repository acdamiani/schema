using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("d_cs Script Icon", true), LightIcon("cs Script Icon", true), Category("MonoBehaviour"), Description(
         "Invoke a method specified by a name, at a specified time from now, and repeat after a specified amount of time indefinitely")]
    public class InvokeRepeating : Action
    {
        [Tooltip("MonoBehaviour to invoke the method on")]
        public BlackboardEntrySelector<MonoBehaviour> monoBehavior;

        [Tooltip("Name of method to invoke")] public BlackboardEntrySelector<string> methodName;

        [Tooltip("Time from now to invoke the method")]
        public BlackboardEntrySelector<float> time;

        [Tooltip("The rate at which the method is repeated")]
        public BlackboardEntrySelector<float> repeatRate;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            if (monoBehavior.value == null)
                return NodeStatus.Failure;

            monoBehavior.value.InvokeRepeating(methodName.value, time.value, repeatRate.value);

            return NodeStatus.Success;
        }
    }
}