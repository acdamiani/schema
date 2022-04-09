using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class Enemy : MonoBehaviour
{
    Animator anim;
    NavMeshAgent agent;
    NavMeshPath path;
    Vector2 smoothDeltaPosition = Vector2.zero;
    Vector2 velocity = Vector2.zero;

    void Start()
    {
        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        path = new NavMeshPath();

        // agent.updatePosition = false;
        // agent.updateRotation = false;
        // agent.updateUpAxis = false;
    }

    void Update()
    {
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance && (!agent.hasPath || agent.velocity.sqrMagnitude == 0f))
        {
            anim.SetBool("move", false);
        }
        else
        {
            anim.SetBool("move", true);
        }

        // if (agent.hasPath && agent.path.corners.Length > 0)
        // {
        //     Vector3 v = agent.path.corners[0];
        //     Vector3 vxy = new Vector3(v.x, 0f, v.z);
        //     Vector3 txy = new Vector3(transform.position.x, 0f, transform.position.z);

        //     Quaternion rot = Quaternion.FromToRotation(transform.forward, (vxy - txy).normalized);

        //     transform.rotation *= rot;

        //     if (Vector3.Distance(vxy, txy) > 0.25f)
        //         anim.SetBool("move", true);
        // }
        // else
        // {
        //     anim.SetBool("move", false);
        // }
    }
}