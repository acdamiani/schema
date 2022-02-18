using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Schema.Runtime;

[DarkIcon("c_TransformIcon")]
[LightIcon("c_TransformIcon")]
public class GetPosition : Action
{
    [Tooltip("Use the current Game Object rather than a Blackboard Key")]
    public bool useSelf;
    [Tooltip("GameObject to get position from")]
    public BlackboardEntrySelector<GameObject> gameObject;
    [WriteOnly]
    [DisableDynamicBinding]
    [Tooltip("Key to store position in")]
    public BlackboardEntrySelector<Vector3> positionKey;
    [Tooltip("When toggled, will use local position (relative to parent) instead of world position")]
    public bool local;
    public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
    {
        // if ((!useSelf && gameObject.empty) || positionKey.empty)
        //     return NodeStatus.Failure;

        if (local)
            positionKey.value = useSelf ? agent.transform.localPosition : gameObject.value.transform.localPosition;
        else
            positionKey.value = useSelf ? agent.transform.position : gameObject.value.transform.position;

        return NodeStatus.Success;
    }
    public override List<Error> GetErrors()
    {
        List<Error> ret = new List<Error>();

        if (positionKey.empty)
            ret.Add(new Error("Position Key is empty", Error.Severity.Warning));

        return ret;
    }
}
