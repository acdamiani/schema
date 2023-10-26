using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [Category("Animation"), DarkIcon("Nodes/d_AnimationCurve"), LightIcon("Nodes/AnimationCurve"),
     Description("Adds a key to an animation curve")]
    public class AddCurveKey : Action
    {
        [Tooltip("Animation curve to use for this operation"), WriteOnly] 
        public BlackboardEntrySelector<AnimationCurve> curve;

        [Tooltip("t value to add key at")] public BlackboardEntrySelector<float> t;
        [Tooltip("Key value to add")] public BlackboardEntrySelector<float> key;

        [Tooltip("Entry to store index of new key in"), WriteOnly] 
        public BlackboardEntrySelector<int> index;

        private void OnValidate()
        {
            t.inspectorValue = Mathf.Clamp01(t.inspectorValue);
        }

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            int i = curve.value.AddKey(t.value, key.value);

            index.value = i;

            if (i == -1)
                return NodeStatus.Failure;

            return NodeStatus.Success;
        }
    }
}