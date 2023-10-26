using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("Nodes/d_Wait"), LightIcon("Nodes/Wait"),
     Description("Waits a given number of frames, then resumes execution of the Behavior Tree")]
    public class WaitForFrames : Action
    {
        [Tooltip("The number of frames to wait for")]
        public BlackboardEntrySelector<int> count = new BlackboardEntrySelector<int>(1);

        private void OnValidate()
        {
            count.inspectorValue = Mathf.Clamp(count.inspectorValue, 1, int.MaxValue);
        }

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            WaitForFramesMemory memory = (WaitForFramesMemory)nodeMemory;

            if (memory.count == count.value)
            {
                memory.count = 0;
                return NodeStatus.Success;
            }

            memory.count++;

            return NodeStatus.Running;
        }

        private class WaitForFramesMemory
        {
            public int count;
        }
    }
}