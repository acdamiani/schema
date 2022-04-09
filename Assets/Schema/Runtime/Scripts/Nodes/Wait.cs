using System;
using System.Collections;
using UnityEngine;
using Schema.Runtime;

[DarkIcon("Dark/Wait")]
[LightIcon("Light/Wait")]
[Description("Waits a given number of seconds, then resumes execution of the Behavior Tree")]
public class Wait : Schema.Runtime.Action
{
    class WaitMemory
    {
        public float startTime;
    }
    public float seconds;
    void OnValidate()
    {
        seconds = seconds > 0.001f ? seconds : 0.001f;
    }
    public override void OnNodeEnter(object nodeMemory, SchemaAgent agent)
    {
        WaitMemory mem = (WaitMemory)nodeMemory;
        mem.startTime = Time.time;
    }
    public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
    {
        WaitMemory memory = (WaitMemory)nodeMemory;

        if (Time.time - memory.startTime >= seconds)
        {
            return NodeStatus.Success;
        }
        else
        {
            return NodeStatus.Running;
        }
    }
}
