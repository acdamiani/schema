using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Schema.Runtime;

[DarkIcon("c_TransformIcon")]
[LightIcon("c_TransformIcon")]
public class GetRotation : Action
{
    [Tooltip("Use the current Game Object rather than a Blackboard Key")]
    public bool useSelf;
    [Tooltip("Get rotation in euler angles")]
    public bool eulerAngles = true;
    [Tooltip("GameObject to get position from")]
    public BlackboardEntrySelector<GameObject> gameObject;
    [Tooltip("Key to store euler angles of rotation in")]
    public BlackboardEntrySelector<Vector3> eulerKey;
    [Tooltip("Key to store rotation as Quaternion in")]
    public BlackboardEntrySelector<Quaternion> quaternionKey;
    [Tooltip("When toggled, will use local rotation (relative to parent) instead of world position")]
    public bool local;
    public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
    {
        if (!useSelf && (gameObject.empty || (eulerAngles && eulerKey.empty) || (!eulerAngles && quaternionKey.empty)))
            return NodeStatus.Failure;

        if (local)
        {
            if (eulerAngles)
                eulerKey.value = useSelf ? agent.transform.localEulerAngles : gameObject.value.transform.localEulerAngles;
            else
                quaternionKey.value = useSelf ? agent.transform.localRotation : gameObject.value.transform.localRotation;
        }
        else
        {
            if (eulerAngles)
                eulerKey.value = useSelf ? agent.transform.eulerAngles : gameObject.value.transform.eulerAngles;
            else
                quaternionKey.value = useSelf ? agent.transform.rotation : gameObject.value.transform.rotation;
        }

        return NodeStatus.Success;
    }
}
