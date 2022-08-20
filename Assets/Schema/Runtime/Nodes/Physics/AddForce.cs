using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("d_Rigidbody Icon", true), LightIcon("Rigidbody Icon", true), Category("Physics")]
    public class AddForce : Action
    {
        public ComponentSelector<Rigidbody> rigidbody;
        public BlackboardEntrySelector<Vector3> forceVector;
        public ForceMode forceMode;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            Rigidbody r = agent.GetComponent(rigidbody);

            if (r == null)
                return NodeStatus.Failure;

            r.AddForce(forceVector.value, forceMode);
            return NodeStatus.Success;
        }
    }
}