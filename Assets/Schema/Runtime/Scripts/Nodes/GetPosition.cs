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
        if ((!useSelf && gameObject.empty) || positionKey.empty)
            return NodeStatus.Failure;

        if (local)
        {
            agent.blackboard.SetValue<Vector3>(
                positionKey,
                useSelf ? agent.transform.localPosition : agent.blackboard.GetValue<GameObject>(gameObject).transform.localPosition
            );
        }
        else
        {
            agent.blackboard.SetValue<Vector3>(
                positionKey,
                useSelf ? agent.transform.position : agent.blackboard.GetValue<GameObject>(gameObject).transform.position
            );
        }

        return NodeStatus.Success;
    }
    public override List<Error> GetErrors()
    {
        List<Error> ret = new List<Error>();

        if (!useSelf && gameObject.empty)
            ret.Add(new Error("GameObject Key is empty", Error.Severity.Error));

        if (positionKey.empty)
            ret.Add(new Error("Position Key is empty", Error.Severity.Warning));

        return ret;
    }
}
