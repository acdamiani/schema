using UnityEngine;
using UnityEngine.AI;

namespace Schema.Example
{
    [Name("Custom_ChasePlayer")]
    public class Custom_ChasePlayer : Action
    {
        // The component selector is a special kind of BlackboardSelector. It will automatically get a component on a gameObject,
        // or it can get the component on the current SchemaAgent
        public ComponentSelector<NavMeshAgent> navMeshAgent;

        // This is used to get the player blackboard entry
        public BlackboardEntrySelector<GameObject> player;

        // The distance between the agent and target that the object is considered "caught"
        public BlackboardEntrySelector<float> minDistance;

        // The maximum distance the player can be from the agent before it gives up
        public BlackboardEntrySelector<float> maxDistance;

        // Tick will run once every frame, if the node is running
        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            // Get our agent using the custom ComponentSelector
            NavMeshAgent a = agent.GetComponent(navMeshAgent);

            // The value of our player object. Smart to cache this
            GameObject playerObject = player.value;

            // If it is null, return failure
            if (a == null)
                return NodeStatus.Failure;

            if (!a.SetDestination(playerObject.transform.position))
                // Setting the destination wasn't successful, return failure
                return NodeStatus.Failure;

            // Distance between the agent and the player object
            float distance = Vector3.Distance(playerObject.transform.position, a.transform.position);

            // Make sure minDistance is above zero
            float min = Mathf.Max(minDistance.value, 0f);

            // Make sure maxDistance is above zero
            float max = Mathf.Max(maxDistance.value, 0f);

            // Reached the player!
            if (distance <= min)
            {
                // Cancel the path since our destination is reached
                a.ResetPath();

                return NodeStatus.Success;
            }
            // Too far away, give up

            if (distance >= max)
            {
                // Cancel the path since we lost the player
                a.ResetPath();

                return NodeStatus.Failure;
            }

            // Still moving towards the player
            return NodeStatus.Running;
        }
    }
}