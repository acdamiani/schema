using System.Collections.Generic;

[System.Serializable]
public class NavMeshAreaFilter
{
    public List<string> areas = new List<string>();
    public int mask;
}