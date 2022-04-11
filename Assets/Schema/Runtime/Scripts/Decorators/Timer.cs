using Schema;
using UnityEngine;
public class Timer : Schema.Decorator
{
    class TimerMemory
    {
        public float timerStarted;
    }
    public float time;
    public override void OnFlowEnter(object decoratorMemory, SchemaAgent agent)
    {
        TimerMemory memory = (TimerMemory)decoratorMemory;

        memory.timerStarted = Time.time;
    }
    public override bool OnNodeProcessed(object decoratorMemory, SchemaAgent agent, ref NodeStatus status)
    {
        TimerMemory memory = (TimerMemory)decoratorMemory;

        if (Time.time - memory.timerStarted <= time)
            return true;
        else
            return false;
    }
}