using System;
using System.Collections.Generic;
using Schema;
using Schema.Runtime;
using Schema.Utilities;
public class BlackboardCompare : Decorator
{
    public BlackboardEntrySelector entryOne = new BlackboardEntrySelector();
    public BlackboardEntrySelector entryTwo = new BlackboardEntrySelector();
    public ComparisonType comparisonTypes = ComparisonType.Equal;
    public float epsilon = 0.1f;
    private Dictionary<ComparisonType, Dictionary<Type, Type>> lookupTable;
    private void OnEnable()
    {
        entryOne.AddAllFilters();
        OnValidate();
    }
    private void OnValidate()
    {
        BlackboardEntry entryOneEditor = entryOne.GetEditorEntry();

        if (entryOneEditor != null && Type.GetType(entryOneEditor.typeString).IsNumeric())
        {
            entryTwo.ClearFilters();
            entryTwo.AddNumericFilter();
        }
        else
        {
            entryTwo.AddAllFilters();
        }
    }
    public override bool Evaluate(object decoratorMemory, SchemaAgent agent)
    {
        object val1 = entryOne.value;
        object val2 = entryTwo.value;

        Type t1 = val1.GetType();
        Type t2 = val2.GetType();

        return Compare(t1, t2, val1, val2);
    }
    public bool Compare(Type t1, Type t2, object val1, object val2)
    {
        //compare with epsilon
        if ((t1.IsNumeric() && t2.IsNumeric()) && (t1.IsDecimal() || t2.IsDecimal()))
        {
            double v1 = Convert.ToDouble(val1);
            double v2 = Convert.ToDouble(val2);

            switch (comparisonTypes)
            {
                case ComparisonType.Equal:
                    return Abs(v1 - v2) < epsilon;
                case ComparisonType.NotEqual:
                    return Abs(v1 - v2) > epsilon;
                case ComparisonType.GreaterThan:
                    return (v1 > v2);
                case ComparisonType.LessThan:
                    return (v1 < v2);
                case ComparisonType.GreaterThanOrEqual:
                    return ((v1 > v2) || (Abs(v1 - v2) < epsilon));
                case ComparisonType.LessThanOrEqual:
                    return ((v1 < v2) || (Abs(v1 - v2) < epsilon));
            }
        }
        else if (t1.IsNumeric() && t2.IsNumeric())
        {
            long v1 = Convert.ToInt64(val1);
            long v2 = Convert.ToInt64(val2);

            switch (comparisonTypes)
            {
                case ComparisonType.Equal:
                    return v1 == v2;
                case ComparisonType.NotEqual:
                    return v1 != v2;
                case ComparisonType.GreaterThan:
                    return v1 > v2;
                case ComparisonType.LessThan:
                    return v1 < v2;
                case ComparisonType.GreaterThanOrEqual:
                    return v1 >= v2;
                case ComparisonType.LessThanOrEqual:
                    return v1 <= v2;
            }
        }
        else
        {
            return comparisonTypes == ComparisonType.Equal ? val1.Equals(val2) : !val1.Equals(val2);
        }

        return false;
    }
    double Abs(double v)
    {
        return v >= 0 ? v : -v;
    }
    [Flags]
    public enum ComparisonType
    {
        Equal = 1,
        NotEqual = 2,
        LessThan = 4,
        GreaterThan = 8,
        LessThanOrEqual = 16,
        GreaterThanOrEqual = 32
    }
}