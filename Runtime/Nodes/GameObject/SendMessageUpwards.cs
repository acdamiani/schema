using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("d_GameObject Icon", true), LightIcon("GameObject Icon", true), Category("GameObject"), Description(
         "Calls a named method on every MonoBehaviour in a specified GameObject and on every ancestor of the behaviour.")]
    public class SendMessageUpwards : Action
    {
        [Tooltip("GameObject to send the message to")]
        public BlackboardEntrySelector<GameObject> gameObject;

        [Tooltip("Name of method to invoke")] public BlackboardEntrySelector<string> methodName;

        [Tooltip("Optional argument to pass to the method")]
        public BlackboardEntrySelector param = new BlackboardEntrySelector();

        [Tooltip("Should an error be raised if the method doesn't exist on the target object?")]
        public SendMessageOptions options;

        protected override void OnObjectEnable()
        {
            param.ApplyAllFilters();

            ;
        }

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            if (gameObject.value == null)
                return NodeStatus.Failure;

            gameObject.value.SendMessageUpwards(methodName.value, param.value, options);

            return NodeStatus.Success;
        }
    }
}