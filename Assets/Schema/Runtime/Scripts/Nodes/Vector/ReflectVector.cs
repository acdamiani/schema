using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Schema;

namespace Schema.Builtin.Nodes
{
    [Description("Reflects a vector off another vector")]
    [Category("Vector")]
    public class ReflectVector : Action
    {
        [Tooltip("Vector A")]
        public BlackboardEntrySelector vectorOne = new BlackboardEntrySelector();
        [Tooltip("Vector B")]
        public BlackboardEntrySelector vectorTwo = new BlackboardEntrySelector();
        [Tooltip("Blackboard variable to store the new reflected vector in"), WriteOnly]
        public BlackboardEntrySelector reflected = new BlackboardEntrySelector();
        protected override void OnNodeEnable()
        {
            vectorOne.ApplyFilters(typeof(Vector2), typeof(Vector3));
            vectorTwo.ApplyFilters(typeof(Vector2), typeof(Vector3));
        }
        void OnValidate()
        {
            int l = 0;

            switch (vectorOne.entryType?.Name)
            {
                case "Vector2":
                    l = Mathf.Max(l, 2);
                    break;
                case "Vector3":
                    l = Mathf.Max(l, 3);
                    break;
            }

            switch (vectorTwo.entryType?.Name)
            {
                case "Vector2":
                    l = Mathf.Max(l, 2);
                    break;
                case "Vector3":
                    l = Mathf.Max(l, 3);
                    break;
            }

            switch (l)
            {
                case 2:
                    reflected.ApplyFilters(typeof(Vector2), typeof(Vector3));
                    break;
                case 3:
                    reflected.ApplyFilters(typeof(Vector3));
                    break;
            }
        }
        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            reflected.value = Vector3.Reflect((Vector3)vectorOne.value, (Vector3)vectorTwo.value);

            return NodeStatus.Success;
        }
    }
}