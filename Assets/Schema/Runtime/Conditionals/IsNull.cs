using System;
using System.Text;
using UnityEngine;

namespace Schema.Builtin.Conditionals
{
    [DarkIcon("Conditionals/d_IsNull"), LightIcon("Conditionals/IsNull")]
    public class IsNull : Conditional
    {
        [Tooltip("Entry to check for null")] public BlackboardEntrySelector entry = new BlackboardEntrySelector();

        protected override void OnObjectEnable()
        {
            entry.ApplyAllFilters();
        }

        public override void OnInitialize(object decoratorMemory, SchemaAgent agent)
        {
            IsNullMemory memory = (IsNullMemory)decoratorMemory;

            Type mapped = EntryType.GetMappedType(entry.entryType);

            if (mapped != null)
                memory.doReturn = mapped.IsValueType;
            else
                memory.doReturn = false;
        }

        public override bool Evaluate(object decoratorMemory, SchemaAgent agent)
        {
            IsNullMemory memory = (IsNullMemory)decoratorMemory;

            if (memory.doReturn)
                return true;

            return entry.value == null;
        }

        public override GUIContent GetConditionalContent()
        {
            StringBuilder sb = new StringBuilder();

            if (entry.isDynamic)
                sb.Append("If dynamic variable ");
            else
                sb.Append("If variable ");

            sb.AppendFormat("<color=red>{0}</color> ", entry.name);

            if (invert)
                sb.Append("is not null");
            else
                sb.Append("is null");

            return new GUIContent(sb.ToString());
        }

        private class IsNullMemory
        {
            public bool doReturn;
        }
    }
}