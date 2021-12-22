using UnityEngine;
using Schema.Runtime;
using System.Collections.Generic;
using System.Linq;

public class Raycast : Decorator
{
    public Vector3 offset;
    public Vector3 direction;
    public float maxDistance;
    public BlackboardEntrySelector point = new BlackboardEntrySelector();
    public RaycastType type;
    public TagFilter tagFilter;
    public bool visualize = true;
    private void OnEnable()
    {
        point.AddObjectFilter();
        point.AddVector2Filter();
        point.AddVector3Filter();
    }
    public override bool Evaluate(object decoratorMemory, SchemaAgent agent)
    {
        if (string.IsNullOrEmpty(point.entryID))
            return false;

        return TestCone(agent, agent.GetBlackboardData());
    }
    public override void DrawGizmos(SchemaAgent agent)
    {
        if (!visualize || type == RaycastType.Dynamic) return;

        Color col = Gizmos.color;
        Vector3 rotatedOffset = agent.transform.rotation * offset;
        Vector3 rotatedDir = agent.transform.rotation * (Quaternion.Euler(direction) * Vector3.forward);

        if (TestCone(agent, null))
            Gizmos.color = Color.green;
        else
            Gizmos.color = Color.white;

        Gizmos.DrawRay(agent.transform.position + rotatedOffset, rotatedDir * maxDistance);

        Gizmos.color = col;
    }
    private bool TestCone(SchemaAgent agent, BlackboardData data)
    {
        RaycastHit[] hits;

        if (type == RaycastType.Absolute)
        {
            Vector3 rotatedOffset = agent.transform.rotation * offset;
            Vector3 rotatedDir = agent.transform.rotation * (Quaternion.Euler(direction) * Vector3.forward);

            hits = Physics.RaycastAll(agent.transform.position + rotatedOffset, rotatedDir, maxDistance);
        }
        else
        {
            if (data == null) return false;

            Vector3 p = GetPoint(point, data);
            hits = Physics.RaycastAll(agent.transform.position, (p - agent.transform.position).normalized);
        }

        return hits.Any(hit => tagFilter.tags.Contains(hit.transform.tag));
    }
    private Vector3 GetPoint(BlackboardEntrySelector selector, BlackboardData data)
    {
        System.Type t = data.GetEntryType(selector.entryID);
        object value = data.GetValue(selector.entryID);

        if (value == null) return Vector3.zero;

        //Not ideal to run every frame, so will be cached in the node state	
        if (t == typeof(GameObject))
        {
            return ((GameObject)value).transform.position;
        }
        else if (t == typeof(Vector2))
        {
            return (Vector2)value;
        }
        else if (t == typeof(Vector2Int))
        {
            return (Vector2)value;
        }
        else if (t == typeof(Vector3))
        {
            return (Vector3)value;
        }
        else if (t == typeof(Vector3Int))
        {
            return (Vector3)value;
        }
        else
        {
            return Vector3.zero;
        }
    }
    public override List<Error> GetErrors()
    {
        List<Error> errors = new List<Error>();

        if (type == RaycastType.Dynamic && string.IsNullOrEmpty(point.entryID))
            errors.Add(new Error("Raycast is marked as dynamic but no valid key picked", Error.Severity.Warning));

        return errors;
    }
    public enum RaycastType
    {
        Absolute,
        Dynamic
    }
}