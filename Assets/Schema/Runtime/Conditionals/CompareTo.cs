using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Schema.Builtin.Conditionals
{
    [DarkIcon("Conditionals/d_Compare")]
    [LightIcon("Conditionals/Compare")]
    public class CompareTo : Conditional
    {
        public enum ComparisonType
        {
            Equal,
            GreaterThan,
            GreaterThanOrEqual,
            LessThan,
            LessThanOrEqual
        }

        [Tooltip("LHS of the comparison")] public BlackboardEntrySelector<float> valueOne;
        [Tooltip("RHS of the comparison")] public BlackboardEntrySelector<float> valueTwo;

        [Tooltip("The comparison type for this operation")]
        public ComparisonType comparisonType;

        public override bool Evaluate(object decoratorMemory, SchemaAgent agent)
        {
            Debug.Log(valueOne.value);

            switch (comparisonType)
            {
                case ComparisonType.Equal:
                    return valueOne.value == valueTwo.value;
                case ComparisonType.GreaterThan:
                    return valueOne.value > valueTwo.value;
                case ComparisonType.GreaterThanOrEqual:
                    return valueOne.value >= valueTwo.value;
                case ComparisonType.LessThan:
                    return valueOne.value < valueTwo.value;
                case ComparisonType.LessThanOrEqual:
                    return valueOne.value <= valueTwo.value;
            }

            return false;
        }

        public override GUIContent GetConditionalContent()
        {
            StringBuilder sb = new();

            sb.AppendFormat("If <color=red>{0}</color> is ", valueOne.name);

            string cName = Regex.Replace(comparisonType.ToString(), "(\\B[A-Z])", " $1").ToLower();

            switch (comparisonType)
            {
                case ComparisonType.Equal:
                case ComparisonType.LessThanOrEqual:
                case ComparisonType.GreaterThanOrEqual:
                    sb.AppendFormat("{0} to ", cName);
                    break;
                case ComparisonType.GreaterThan:
                case ComparisonType.LessThan:
                    sb.AppendFormat("{0} ", cName);
                    break;
            }

            sb.AppendFormat("<color=red>{0}</color>", valueTwo.name);

            return new GUIContent(sb.ToString());
        }

        private class IsNullMemory
        {
            public bool doReturn;
        }
    }
}