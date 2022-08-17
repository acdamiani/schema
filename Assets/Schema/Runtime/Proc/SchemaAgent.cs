using Schema;
using Schema.Internal;
using Schema.Utilities;
using UnityEngine;

namespace Schema
{
    public class SchemaAgent : MonoBehaviour
    {
        private static readonly CacheDictionary<Graph, ExecutableTree> treeMap = new();

        [SerializeField][Tooltip("Target Graph asset for this agent")] private Graph m_target;
        [SerializeField][TextArea][Tooltip("Description for this agent that will be displayed in the Node View")] private string m_agentDescription;
        [SerializeField][Min(1)][Tooltip("Maximum number of steps to be taken throughout the tree before a forceful exit")] private int m_maxStepsPerTick = 1000;
        [SerializeField][Tooltip("Restart the tree when complete")] private bool m_restartWhenComplete = true;
        [SerializeField][Tooltip("Reset all blackboard values (excluding globals) when the tree restarts")] private bool m_resetBlackboardOnRestart;
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

        public bool restartWhenComplete
        {
            get => m_restartWhenComplete;
            set => m_restartWhenComplete = value;
        }

        public bool resetBlackboardOnRestart
        {
            get => m_resetBlackboardOnRestart;
            set => m_resetBlackboardOnRestart = value;
        }

        public string agentDescription
        {
            get => m_agentDescription;
            set => m_agentDescription = value;
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
}