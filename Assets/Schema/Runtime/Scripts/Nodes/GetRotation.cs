using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Schema.Runtime;

public class GetRotation : Action
{
    [Tooltip("Use the current Game Object rather than a Blackboard Key")]
    public bool useSelf;
    [Tooltip("Get rotation in euler angles")]
    public bool eulerAngles = true;
    [Tooltip("GameObject to get position from")]
    public BlackboardGameObject gameObject;
    [Tooltip("Key to store euler angles of rotation in")]
    public BlackboardVector3 eulerKey;
    [Tooltip("Key to store rotation as Quaternion in")]
    public BlackboardQuaternion quaternionKey;
    [Tooltip("When toggled, will use local rotation (relative to parent) instead of world position")]
    public bool local;
    public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
    {
        if (!useSelf && (gameObject.empty || (eulerAngles && eulerKey.empty) || (!eulerAngles && quaternionKey.empty)))
            return NodeStatus.Failure;

        if (local)
        {
            if (eulerAngles)
            {
                agent.blackboard.SetValue<Vector3>(
                    eulerKey,
                    useSelf ? agent.transform.localEulerAngles : agent.blackboard.GetValue<GameObject>(gameObject).transform.localEulerAngles
                );
            }
            else
            {
                agent.blackboard.SetValue<Quaternion>(
                    quaternionKey,
                    useSelf ? agent.transform.localRotation : agent.blackboard.GetValue<GameObject>(gameObject).transform.localRotation
                );
            }
        }
        else
        {
            if (eulerAngles)
            {
                agent.blackboard.SetValue<Vector3>(
                    eulerKey,
                    useSelf ? agent.transform.eulerAngles : agent.blackboard.GetValue<GameObject>(gameObject).transform.eulerAngles
                );
            }
            else
            {
                agent.blackboard.SetValue<Quaternion>(
                    quaternionKey,
                    useSelf ? agent.transform.rotation : agent.blackboard.GetValue<GameObject>(gameObject).transform.rotation
                );
            }
        }

        return NodeStatus.Success;
    }
}
