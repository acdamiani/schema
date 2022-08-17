using Schema;
using Schema.Internal;
using Schema.Utilities;
using UnityEngine;

public class SchemaAgent : MonoBehaviour
{
    private static readonly CacheDictionary<Graph, ExecutableTree> treeMap = new();

    [SerializeField] private Graph m_target;
    [SerializeField] [Min(1)] private int m_maxStepsPerTick = 1000;
    private float t;

    public Graph target
    {
        get => m_target;
        set => m_target = value;
    }

    public int maxStepsPerTick
    {
        get => m_maxStepsPerTick;
        set => m_maxStepsPerTick = value;
    }

    public ExecutableTree tree { get; private set; }

    public void Start()
    {
        if (m_target == null)
            return;

        tree = treeMap.GetOrCreate(m_target, () => new ExecutableTree(m_target));
        tree.Initialize(this);

        t = Time.time + m_maxStepsPerTick;
    }

    public void Update()
    {
        if (tree == null)
            return;

        tree.Tick(this);
    }
}