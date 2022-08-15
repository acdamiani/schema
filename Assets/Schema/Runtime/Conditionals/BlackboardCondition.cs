using System;

namespace Schema.Builtin.Conditionals
{
    [Serializable]
    public class BlackboardCondition : Conditional
    {
        public enum ConditionType
        {
            IsSet,
            IsNotSet
        }

        public BlackboardEntrySelector blackboardKey = new();
        public ConditionType conditionType;

        protected override void OnObjectEnable()
        {
            blackboardKey.ApplyAllFilters();
        }

        public override bool Evaluate(object decoratorMemory, SchemaAgent agent)
        {
            object val = blackboardKey.value;
            bool isSet = val != null;

            bool ret = conditionType == ConditionType.IsSet ? isSet : !isSet;

            return ret;
        }
    }
}