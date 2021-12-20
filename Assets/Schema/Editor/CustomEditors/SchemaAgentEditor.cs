using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SchemaAgent))]
public class SchemaAgentEditor : Editor
{
    [SerializeField] bool generalFoldout;
    [SerializeField] bool advancedFoldout;
    public override void OnInspectorGUI()
    {
        SchemaAgent agent = (SchemaAgent)target;

        EditorGUI.BeginChangeCheck();
        agent.target = (Schema.Runtime.Graph)EditorGUILayout.ObjectField("Target", agent.target, typeof(Schema.Runtime.Graph), false);
        if (EditorGUI.EndChangeCheck())
        {
            agent.VerifyComponents();
        }

        generalFoldout = EditorGUILayout.Foldout(generalFoldout, "General Settings", true, EditorStyles.foldoutHeader);

        if (generalFoldout)
        {
            agent.restartOnComplete = EditorGUILayout.Toggle(new GUIContent("Restart On Complete", "Restart tree when execution has finished (when the engine has reached the root)"), true);
            agent.maxIterationsPerTick = EditorGUILayout.IntField(new GUIContent("Max Iterations Per Tick", "Maximum number of steps taken through the tree in a single tick."), agent.maxIterationsPerTick);
            agent.maxIterationsPerTick = Mathf.Clamp(agent.maxIterationsPerTick, 0, System.Int32.MaxValue);
        }

        advancedFoldout = EditorGUILayout.Foldout(advancedFoldout, "Advanced Settings", true, EditorStyles.foldoutHeader);

        if (advancedFoldout)
        {
            agent.ticksPerSecond = Mathf.Clamp(EditorGUILayout.IntField(
                new GUIContent("TPS", "Ticks Per Second, or the how many times the behavior tree will execute within a given second. The recommended value is 60"),
                agent.ticksPerSecond), 1, 120);
            agent.checksPerSecond = Mathf.Clamp(EditorGUILayout.IntField(
                new GUIContent("CPS", "Checks Per Second, or how many times per second the engine will evaluate relevant decorators and abort the tree if necessary. It is recommended that this have the same value as the TPS of the tree"),
                agent.checksPerSecond), 1, 120);

            agent.logTaskChanges = EditorGUILayout.Toggle(new GUIContent("Log Task Changes", "When checked, will log whenever the task has changed to the console"), agent.logTaskChanges);
            agent.ignoreTickOverstep = EditorGUILayout.Toggle(new GUIContent("Ignore Tick Overstep", "Ignore the overstepping of the tick limit for this agent. It is highly recommended that this stay off, as checking it will no longer stop infinite loops"), agent.ignoreTickOverstep);
        }
    }
}