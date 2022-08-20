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
        [SerializeField][Min(0)][Tooltip("Amount of time in seconds to pause between executions of the tree")] private float m_treePauseTime;
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

        public float treePauseTime
        {
            get => m_treePauseTime;
            set => m_treePauseTime = value;
        }

        /// <summary>
        /// Whether the current tree is paused - will return true during gaps caused by treePauseTime
        /// </summary>
        public bool paused { get { return _paused; } }
        private bool _paused;

        /// <summary>
        /// Whether the current tree is stopped 
        /// </summary>
        public bool stopped { get { return _stopped; } }
        private bool _stopped;

        /// <summary>
        /// The executable tree for this agent
        /// </summary>
        public ExecutableTree tree { get { return _tree; } }
        private ExecutableTree _tree;

        private void Start()
        {
            if (m_target == null)
                return;

            _tree = treeMap.GetOrCreate(m_target, () => new ExecutableTree(m_target));
            _tree.Initialize(this);
        }

        private void Update()
        {
            if (_paused && Time.time >= t)
                Resume();

            if (_tree == null)
                return;

            _tree.Tick(this);
        }

        /// <summary>
        /// Pause the tree's execution
        /// </summary>
        public void Pause()
        {
            _paused = true;
        }

        /// <summary>
        /// Pause the tree's execution for a given number of seconds
        /// </summary>
        /// <param name="time">Number of seconds to pause the tree for</param>
        public void Pause(float time)
        {
            if (time <= 0f)
                throw new System.ArgumentOutOfRangeException("time", "Time was less than or equal to zero seconds! Skipping...");

            _paused = true;

            t = Time.time + time;
        }

        /// <summary>
        /// Resume the tree's execution immediately
        /// </summary>
        public void Resume()
        {
            _paused = false;

            t = 0f;
        }

        /// <summary>
        /// Stop the tree's execution permanently. Cannot be resumed by Resume()
        /// </summary>
        public void Stop()
        {
            _stopped = true;
        }

        /// <summary>
        /// Restarts the tree's execution
        /// </summary>
        public void Reset()
        {
            if (_tree == null)
                return;

            _tree.GetExecutionContext(this).node = _tree.root;
        }

        /// <summary>
        /// Restarts the tree's execution and optionally reset its blackboard
        /// </summary>
        /// <param name="resetBlackboard">Whether to reset the blackboard when resetting the tree</param>
        public void Reset(bool resetBlackboard)
        {
            if (_tree == null)
                return;

            _tree.GetExecutionContext(this).node = _tree.root;

            if (resetBlackboard)
                _tree.blackboard.Reset();
        }
    }
}