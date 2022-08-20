using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("d_Animator Icon", true), LightIcon("Animator Icon", true), Category("Animator")]
    public class SetAnimatorVariable : Action
    {
        public ComponentSelector<Animator> animator;
        public AnimatorControllerParameterType type = AnimatorControllerParameterType.Float;
        public BlackboardEntrySelector<string> parameterName;
        public BlackboardEntrySelector<float> floatValue;
        public BlackboardEntrySelector<int> intValue;
        public BlackboardEntrySelector<bool> boolValue;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            Animator a = animator.GetValue(agent);

            if (a == null)
                return NodeStatus.Failure;

            switch (type)
            {
                case AnimatorControllerParameterType.Int:
                    a.SetInteger(parameterName.value, intValue.value);
                    break;
                case AnimatorControllerParameterType.Float:
                    a.SetFloat(parameterName.value, floatValue.value);
                    break;
                case AnimatorControllerParameterType.Bool:
                    a.SetBool(parameterName.value, boolValue.value);
                    break;
                case AnimatorControllerParameterType.Trigger:
                    a.SetTrigger(parameterName.value);
                    break;
            }


            return NodeStatus.Success;
        }
    }
}