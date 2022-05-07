using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Schema;

namespace Schema.Builtin.Nodes
{
    [Description("Normalize a Vector")]
    public class NormalizeVector : Action
    {
        [Tooltip("Vector to normalize")]
        public BlackboardEntrySelector vectorOne = new BlackboardEntrySelector();
        [Tooltip("Blackboard variable to store the normalized vector in"), WriteOnly]
        public BlackboardEntrySelector normalized = new BlackboardEntrySelector();
        protected override void OnNodeEnable()
        {
            vectorOne.ApplyFilters(typeof(Vector2), typeof(Vector3), typeof(Vector4));
        }
        void OnValidate()
        {
            switch (vectorOne.entryType?.Name)
            {
                case "Vector2":
                    normalized.ApplyFilter<Vector2>();
                    break;
                case "Vector3":
                    normalized.ApplyFilter<Vector3>();
                    break;
                case "Vector4":
                    normalized.ApplyFilter<Vector4>();
                    break;
            }
        }
        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            normalized.value = Vector4.Normalize((Vector4)vectorOne.value);

            return NodeStatus.Success;
        }
    }
}