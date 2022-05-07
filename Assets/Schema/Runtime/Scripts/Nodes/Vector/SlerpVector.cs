using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Schema;

namespace Schema.Builtin.Nodes
{
    [Description("Spherically interpolate between two Vectors")]
    public class SlerpVector : Action
    {
        [Tooltip("Vector A")]
        public BlackboardEntrySelector vectorOne = new BlackboardEntrySelector();
        [Tooltip("Vector B")]
        public BlackboardEntrySelector vectorTwo = new BlackboardEntrySelector();
        [Tooltip("Amount to interpolate by")]
        public BlackboardEntrySelector<float> t = new BlackboardEntrySelector<float>();
        [Tooltip("Whether to clamp the t value")]
        public bool unclamped;
        [Tooltip("Blackboard variable to store the slerped vector in."), WriteOnly]
        public BlackboardEntrySelector slerped = new BlackboardEntrySelector();
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
                    slerped.ApplyFilters(typeof(Vector2), typeof(Vector3));
                    break;
                case 3:
                    slerped.ApplyFilters(typeof(Vector3));
                    break;
            }

            if (!unclamped)
                t.inspectorValue = Mathf.Clamp01(t.inspectorValue);
        }
        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            if (unclamped)
                slerped.value = Vector3.SlerpUnclamped((Vector3)vectorOne.value, (Vector3)vectorTwo.value, t.value);
            else
                slerped.value = Vector3.Slerp((Vector3)vectorOne.value, (Vector3)vectorTwo.value, t.value);

            return NodeStatus.Success;
        }
    }
}