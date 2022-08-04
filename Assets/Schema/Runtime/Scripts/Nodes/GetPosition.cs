using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Schema;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("d_Transform Icon", true)]
    [LightIcon("Transform Icon", true)]
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
    }
}