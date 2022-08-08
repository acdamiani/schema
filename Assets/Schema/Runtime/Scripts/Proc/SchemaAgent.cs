using UnityEngine;
using Schema;
using Schema.Utilities;
using Schema.Internal;
using System.Collections.Generic;

public class SchemaAgent : UnityEngine.MonoBehaviour
{
    public Graph target { get { return m_target; } set { m_target = value; } }
    [SerializeField] private Graph m_target;
    public ExecutableTree tree { get { return m_tree; } }
    private ExecutableTree m_tree;
    private static CacheDictionary<Graph, ExecutableTree> treeMap
        = new CacheDictionary<Graph, ExecutableTree>();
    public void Start()
    {
        if (m_target == null)
            return;

        m_tree = treeMap.GetOrCreate(m_target, () => new ExecutableTree(m_target));
        m_tree.Initialize(this);
    }
    public void Update()
    {
        if (tree == null)
            return;

        m_tree.Tick(this);
    }
}