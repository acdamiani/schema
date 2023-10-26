using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("d_random"), LightIcon("random"), Category("Random"), Description("Get a random value in a range")]
    public class RandomRange : Action
    {
        public BlackboardEntrySelector target = new BlackboardEntrySelector();

        [Tooltip("Mimimum allowed value for the range (inclusive)")]
        public BlackboardEntrySelector<float> floatMin;

        [Tooltip("Maximum allowed value for the range (inclusive)")]
        public BlackboardEntrySelector<float> floatMax = new BlackboardEntrySelector<float>(1f);

        [Tooltip("Mimimum allowed value for the range (inclusive)")]
        public BlackboardEntrySelector<int> intMin;

        [Tooltip("Maximum allowed value for the range (inclusive)")]
        public BlackboardEntrySelector<int> intMax = new BlackboardEntrySelector<int>(1);

        protected override void OnObjectEnable()
        {
            target.ApplyFilters(typeof(int), typeof(float));
        }

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            if (target.isDynamic)
                target.value = Random.Range(floatMin.value, floatMax.value);
            else
                switch (Type.GetTypeCode(target.entryType))
                {
                    case TypeCode.Single:
                        target.value = Random.Range(floatMin.value, floatMax.value);
                        break;
                    case TypeCode.Int32:
                        target.value = Random.Range(intMin.value, intMax.value);
                        break;
                }

            return NodeStatus.Success;
        }
    }
}