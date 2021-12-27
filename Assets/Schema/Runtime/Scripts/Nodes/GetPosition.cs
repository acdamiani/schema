using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Schema.Runtime;

public class GetPosition : Action
{
    [Tooltip("Use the current Game Object rather than a Blackboard Key")]
    public bool useSelf;
    [Tooltip("GameObject to get position from")]
    public BlackboardGameObject gameObject;
    [Tooltip("Key to store position in")]
    public BlackboardVector3 positionKey;
    [Tooltip("When toggled, will use local position (relative to parent) instead of world position")]
    public bool local;
    public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
    {
        agent.blackboard.SetValue<Vector3>(positionKey, agent.transform.position);

        return NodeStatus.Success;
    }
}
