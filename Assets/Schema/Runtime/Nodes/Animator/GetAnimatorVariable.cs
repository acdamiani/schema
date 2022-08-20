using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("d_Animator Icon", true), LightIcon("Animator Icon", true), Category("Animator")]
    public class GetAnimatorVariable : Action
    {
        public enum GetAnimatorVariableType
        {
            Float,
            Int,
            Bool
        }

        public ComponentSelector<Animator> animator;
        public GetAnimatorVariableType type;
        public BlackboardEntrySelector<string> parameterName;
        [WriteOnly] public BlackboardEntrySelector<float> floatValue;
        [WriteOnly] public BlackboardEntrySelector<int> intValue;
        [WriteOnly] public BlackboardEntrySelector<bool> boolValue;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            Animator a = agent.GetComponent(animator);

            if (a == null)
                return NodeStatus.Failure;

            switch (type)
            {
                case GetAnimatorVariableType.Int:
                    intValue.value = a.GetInteger(parameterName.value);
                    break;
                case GetAnimatorVariableType.Float:
                    floatValue.value = a.GetFloat(parameterName.value);
                    break;
                case GetAnimatorVariableType.Bool:
                    boolValue.value = a.GetBool(parameterName.value);
                    break;
            }

            return NodeStatus.Success;
        }
    }
}