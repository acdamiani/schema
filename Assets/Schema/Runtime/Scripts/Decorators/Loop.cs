using UnityEngine;
using Schema;

[AllowOnlyOne]
internal class Loop : Decorator
{
    public class LoopMemory
    {
        //Count always starts at one (since we are guaranteed that the node ran at least once)
        public int currentCount = 1;
    }
    public bool loopForInfinity;
    public BlackboardEntrySelector<int> count;
    private void OnValidate()
    {
        count.inspectorValue = Mathf.Clamp(count.inspectorValue, 1, 1000);
    }
    public override bool OnNodeProcessed(object decoratorMemory, SchemaAgent agent, ref NodeStatus status)
    {
        LoopMemory memory = (LoopMemory)decoratorMemory;

        if ((memory.currentCount < count.value) || loopForInfinity)
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
    public override GUIContent GetInfoContent()
    {
        if (loopForInfinity)
        {
            return new GUIContent($"Will loop node for infinity");
        }
        else
        {
            if (count.empty)
                return new GUIContent($"Will loop node {count.inspectorValue} time{(count.inspectorValue > 1 ? "s" : "")}");
            else
                return new GUIContent($"Will loop node for the value of {count.entry.name} times");
        }
    }
}