using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Schema.Runtime;

[DarkIcon("c_TransformIcon")]
[LightIcon("c_TransformIcon")]
public class GetScale : Action
{
    [Tooltip("Use the current Game Object rather than a Blackboard Key")]
    public bool useSelf;
    [Tooltip("GameObject to get scale from")]
    public BlackboardEntrySelector<GameObject> gameObject;
    [Tooltip("Key to store scale in")]
    public BlackboardEntrySelector<Vector3> scaleKey;
    [Tooltip("When toggled, will use local scale (relative to parent) instead of lossy scale")]
    public bool local;
    public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
    {
        if ((!useSelf && gameObject.empty) || scaleKey.empty)
            return NodeStatus.Failure;

        if (local)
        {
            agent.blackboard.SetValue<Vector3>(
                scaleKey,
                useSelf ? agent.transform.lossyScale : agent.blackboard.GetValue<GameObject>(gameObject).transform.lossyScale
            );
        }
        else
        {
            agent.blackboard.SetValue<Vector3>(
                scaleKey,
                useSelf ? agent.transform.localScale : agent.blackboard.GetValue<GameObject>(gameObject).transform.localScale
            );
        }

        return NodeStatus.Success;
    }
}
