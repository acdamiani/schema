using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Schema;

[DarkIcon("c_Rigidbody")]
[LightIcon("c_Rigidbody")]
public class AddTorque : Action
{
    public ComponentSelector<Rigidbody> rigidbody;
    public BlackboardEntrySelector<Vector3> torque;
    public ForceMode forceMode;
    public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
    {
        Rigidbody r = agent.GetComponent(rigidbody);

        if (r != null)
        {
            r.AddTorque(torque.value, forceMode);
            return NodeStatus.Success;
        }
        else
        {
            return NodeStatus.Failure;
        }
    }
}
