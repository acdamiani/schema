using UnityEngine;
using Schema;

[AllowOnlyOne]
public class Loop : Decorator
{
    [Info]
    public string times => $"Will loop node {(loopForInfinity ? "for infinity" : $"{count} time{(count > 1 ? "s" : "")}")}";
    public class LoopMemory
    {
        //Count always starts at one (since we are guaranteed that the node ran at least once)
        public int currentCount = 1;
    }
    public bool loopForInfinity;
    public int count = 1;
    private void OnValidate()
    {
        count = Mathf.Clamp(count, 1, 1000);
    }
    public override bool OnNodeProcessed(object decoratorMemory, SchemaAgent agent, ref NodeStatus status)
    {
        LoopMemory memory = (LoopMemory)decoratorMemory;

        if ((memory.currentCount < count) || loopForInfinity)
        {
            memory.currentCount++;
            return true;
        }
        else
        {
            memory.currentCount = 1;
            return false;
        }
    }
}