using System.Text;
using UnityEngine;

namespace Schema.Builtin.Conditionals
{
    [DarkIcon("Conditionals/d_Cooldown"), LightIcon("Conditionals/Cooldown"),
     Description("Disable running a node until it a time period has been elapsed since its last run")]
    public class Cooldown : Conditional
    {
        [Tooltip("Time until the node can be run after already being run")]
        public BlackboardEntrySelector<float> cooldownTime;

        public override bool Evaluate(object decoratorMemory, SchemaAgent agent)
        {
            CooldownMemory memory = (CooldownMemory)decoratorMemory;

            if (Time.time - memory.t >= cooldownTime.value)
            {
                memory.t = Time.time;
                return true;
            }

            return false;
        }

        public override GUIContent GetConditionalContent()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("Cooldown for ");

            sb.AppendFormat("<color=red>{0}</color> ", cooldownTime.name);

            sb.Append("seconds");

            return new GUIContent(sb.ToString());
        }

        private class CooldownMemory
        {
            public float t;
        }
    }
}