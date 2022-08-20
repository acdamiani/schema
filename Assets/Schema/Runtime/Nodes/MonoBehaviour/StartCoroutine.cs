using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("d_cs Script Icon", true), LightIcon("cs Script Icon", true), Category("MonoBehaviour"),
     Description("Start a coroutine on a MonoBehavior with a single given argument")]
    public class StartCoroutine : Action
    {
        [Tooltip("MonoBehaviour to stop methods on")]
        public BlackboardEntrySelector<MonoBehaviour> monoBehaviour;

        [Tooltip("Name of the coroutine to start")]
        public BlackboardEntrySelector<string> methodName;

        [Tooltip("Optional argument to be passed to the Coroutine")]
        public BlackboardEntrySelector arg;

        protected override void OnObjectEnable()
        {
            arg.ApplyAllFilters();

            ;
        }

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            if (monoBehaviour.value == null)
                return NodeStatus.Failure;

            monoBehaviour.value.StartCoroutine(methodName.value, arg.value);

            return NodeStatus.Success;
        }
    }
}