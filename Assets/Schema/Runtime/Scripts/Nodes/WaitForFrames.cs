using Schema;
using UnityEngine;

[DarkIcon("Dark/Wait")]
[LightIcon("Light/Wait")]
[Description("Waits a given number of frames, then resumes execution of the Behavior Tree")]
public class WaitForFrames : Action
{
    [Tooltip("The number of frames to wait for")] public BlackboardEntrySelector<int> count;
    class WaitForFramesMemory
    {
        public int count;
    }
    protected override void OnNodeEnable()
    {
        count.inspectorValue = 1;
    }
    void OnValidate()
    {
        count.inspectorValue = Mathf.Clamp(count.inspectorValue, 1, System.Int32.MaxValue);
    }
    public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
    {
        WaitForFramesMemory memory = (WaitForFramesMemory)nodeMemory;

        memory.count++;

        if (memory.count == count.value)
            return NodeStatus.Success;

        return NodeStatus.Running;
    }
}