using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Schema.Runtime;

[DarkIcon("Dark/WaitBlackboardTime")]
[LightIcon("Light/WaitBlackboardTime")]
public class WaitBlackboardTime : Action
{
    class WaitBlackboardTimeMemory
    {
        public float startTime;
        public BlackboardData data;
    }
    public BlackboardNumber number;
    public override void OnInitialize(object nodeMemory, SchemaAgent agent)
    {
        WaitBlackboardTimeMemory memory = (WaitBlackboardTimeMemory)nodeMemory;

        memory.data = agent.GetBlackboardData();
    }
    public override void OnNodeEnter(object nodeMemory, SchemaAgent agent)
    {
        WaitBlackboardTimeMemory memory = (WaitBlackboardTimeMemory)nodeMemory;
        memory.startTime = Time.time;
    }
    public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
    {
        WaitBlackboardTimeMemory memory = (WaitBlackboardTimeMemory)nodeMemory;

        if (string.IsNullOrEmpty(number.entryID)) return NodeStatus.Failure;

        if (Time.time - memory.startTime >= memory.data.GetValue<float>(number.entryID))
        {
            return NodeStatus.Success;
        }
        else
        {
            return NodeStatus.Running;
        }
    }
}
