using UnityEngine;
using Schema.Runtime;

public class Cooldown : Decorator
{
    public float time;
    public bool startCountdownOnExit;
    class CooldownMemory
    {
        public float lastTime = -1f;
    }
    public override bool Evaluate(object decoratorMemory, SchemaAgent agent)
    {
        CooldownMemory memory = (CooldownMemory)decoratorMemory;

        return memory.lastTime < 0 || Time.time - memory.lastTime >= time;
    }
    public override void OnFlowEnter(object decoratorMemory, SchemaAgent agent)
    {
        CooldownMemory memory = (CooldownMemory)decoratorMemory;

        if (!startCountdownOnExit)
            memory.lastTime = Time.time;
    }
    public override void OnFlowExit(object decoratorMemory, SchemaAgent agent)
    {
        CooldownMemory memory = (CooldownMemory)decoratorMemory;

        if (startCountdownOnExit)
            memory.lastTime = Time.time;
    }
}