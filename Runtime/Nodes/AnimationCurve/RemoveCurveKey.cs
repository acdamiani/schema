using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [Description("Removes a key from an animation curve"), DarkIcon("Nodes/d_AnimationCurve"),
     LightIcon("Nodes/AnimationCurve"), Category("Animation")]
    public class RemoveCurveKey : Action
    {
        [Tooltip("Animation curve to use for this operation"), WriteOnly] 
        public BlackboardEntrySelector<AnimationCurve> curve;

        [Tooltip("Index of key to remove")] public BlackboardEntrySelector<int> index;

        private void OnValidate()
        {
            index.inspectorValue = Mathf.Clamp(index.inspectorValue, 0, int.MaxValue);
        }

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            curve.value.RemoveKey(index.value);

            return NodeStatus.Success;
        }
    }
}