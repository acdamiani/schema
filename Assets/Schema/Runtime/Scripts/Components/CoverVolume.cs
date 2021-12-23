using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[ExecuteInEditMode]
public class CoverVolume : MonoBehaviour
{
    public Vector3 center;
    public Vector3 size = Vector3.one;
    public float spacing = .5f;
    public float preferredDist = 1.0f;
    public NavMeshAreaMask filter;
    public Vector3[] GeneratePoints()
    {
        //Generate evenly distributed points inside bounds
        Vector3 root = transform.position + MultiplyComponents(transform.lossyScale, center) - MultiplyComponents(transform.lossyScale, size) / 2f;

        List<Vector3> p = new List<Vector3>();

        float halfX = (size.x * transform.lossyScale.x) / 2f;
        float halfY = (size.y * transform.lossyScale.y) / 2f;
        float halfZ = (size.z * transform.lossyScale.z) / 2f;

        //Through the perils of float math, a small epsilon is required ot ensure that the correct number of points are generated for spacing that should evenly go into size
        int numX = Mathf.FloorToInt(size.x * transform.lossyScale.x / spacing + 0.01f) + 1;
        int numY = Mathf.FloorToInt(size.y * transform.lossyScale.y / spacing + 0.01f) + 1;
        int numZ = Mathf.FloorToInt(size.z * transform.lossyScale.z / spacing + 0.01f) + 1;

        for (int x = 0; x < numX; x++)
        {
            for (int y = 0; y < numY; y++)
            {
                for (int z = 0; z < numZ; z++)
                {
                    Vector3 point = new Vector3(x * spacing - halfX, y * spacing - halfY, z * spacing - halfZ);

                    //Apply transform to Vector to get world position (current point is relative to the center)
                    Matrix4x4 m = Matrix4x4.TRS(transform.position + transform.rotation * MultiplyComponents(transform.lossyScale, center), transform.rotation, Vector3.one);
                    point = m.MultiplyPoint(point);

                    NavMeshHit hit;

                    if (NavMesh.SamplePosition(point, out hit, spacing / 2f, filter.mask))
                    {
                        //Double check that the point is inside the cube
                        Vector3 pos = hit.position;

                        //Multiply position by the inverse of the transform matrix to translate it back into local space
                        pos = m.inverse.MultiplyPoint(pos);

                        //Check to see if point is inside the bounds of the cube
                        if (!PositionInsideCube(pos, Vector3.zero, MultiplyComponents(transform.lossyScale, size), 0.01f))
                            continue;

                        //Then add the unrotated point if it has
                        p.Add(hit.position);
                    }
                }
            }
        }

        return p.ToArray();
    }
    // public Dictionary<Vector3, float> ClassifyPoints(Vector3[] points, GameObject target, GameObject agent)
    // {
    //     //All colliders in the scene
    //     Collider[] colliders = GameObject.FindObjectsOfType<Collider>();

    //     Dictionary<Vector3, float> d = new Dictionary<Vector3, float>();

    //     float diagonal = SqrDiagonalDist(MultiplyComponents(transform.lossyScale, size));

    //     for (int i = 0; i < points.Length; i++)
    //     {
    //         Vector3 point = points[i];

    //         //Factors that invalidate a point
    //         RaycastHit hitInfo;
    //         if (!Physics.Linecast(target.transform.position, point, out hitInfo))
    //         {
    //             d.Add(point, 0f);
    //             continue;
    //         }

    //         //Vector3.Distance(a,b) is the same as (a-b).magnitude
    //         float targetDist = (target.transform.position - point).sqrMagnitude / diagonal;
    //         targetDist = Mathf.Clamp01(targetDist);

    //         float agentDist = (agent.transform.position - point).sqrMagnitude / diagonal;
    //         agentDist = 1f - Mathf.Clamp01(agentDist);

    //         //Get closest collider
    //         float colliderDist = DistToCollider(point) / preferredDist;
    //         colliderDist = 1f - Mathf.Clamp01(colliderDist);

    //         d.Add(points[i], agentDist * 0.25f + targetDist * 0.20f + colliderDist * 0.55f);
    //     }

    //     return d;
    // }
    public float DistToCollider(Vector3 point)
    {
        float stepSize = 0.1f;
        float radius = 0.1f;
        float maxDist = 100f;

        Collider[] results;

        while (radius < maxDist)
        {
            results = Physics.OverlapSphere(point, radius);

            if (results.Length > 0)
                return radius;

            radius += stepSize;
        }

        return maxDist;
    }
    float SqrDiagonalDist(Vector3 size)
    {
        return Mathf.Pow(size.x, 2) + Mathf.Pow(size.y, 2) + Mathf.Pow(size.z, 2);
    }
    Vector3 MultiplyComponents(Vector3 a, Vector3 b)
    {
        return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
    }

    bool PositionInsideCube(Vector3 position, Vector3 cubeCenter, Vector3 cubeSize, float epsilon)
    {
        if (position.x + epsilon < cubeCenter.x - cubeSize.x / 2f || position.x - epsilon > cubeCenter.x + cubeSize.x / 2f)
            return false;

        if (position.y + epsilon < cubeCenter.y - cubeSize.y / 2f || position.y - epsilon > cubeCenter.y + cubeSize.y / 2f)
            return false;

        if (position.z + epsilon < cubeCenter.z - cubeSize.z / 2f || position.z - epsilon > cubeCenter.z + cubeSize.z / 2f)
            return false;

        return true;
    }
}
