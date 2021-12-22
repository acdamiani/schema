using UnityEngine;
using UnityEditor;
using Schema.Runtime;

public class SoundHeard : Decorator
{
    [Tooltip("How sensitive the agent is to hearing sound, in dB. Anthing louder than this number will be considered heard")]
    public float soundSensitivity = 60.0f;
    [Tooltip("Display heard noises locations in the editor for the currently selected agent")]
    public bool visualize = true;
    public BlackboardVector3 hitPoint;
    [Tooltip("The tags to filter from. Only these tags will be considered when listening for noise")]
    public TagFilter tagFilter;
    [Info]
    public string willStore => $"Will store closest sound location in Vector3 {(hitPoint.GetEditorEntry() != null ? hitPoint.GetEditorEntry().Name : "null")}";
    class SoundHeardMemory
    {
        public BlackboardData data;
    }
    public override void OnInitialize(object decoratorMemory, SchemaAgent agent)
    {
        SoundHeardMemory memory = (SoundHeardMemory)decoratorMemory;
        memory.data = agent.GetBlackboardData();
    }
    public override bool Evaluate(object decoratorMemory, SchemaAgent agent)
    {
        return false;
    }
    public override void DrawGizmos(SchemaAgent agent)
    {
        if (!visualize)
            return;

        Vector3[] audioPoints = GetAudio(agent);

        Color handlesColor = Handles.color;

        for (int i = 0; i < audioPoints.Length; i++)
        {
            Vector3 point = audioPoints[i];

            Handles.color = new Color(0f, 1f, 0f, 0.25f);
            Handles.DrawSolidDisc(point, SceneView.lastActiveSceneView.rotation * Vector3.forward, 1f);
        }

        Handles.color = handlesColor;
    }
    private Vector3[] GetAudio(SchemaAgent agent)
    {
        AudioSource[] sources = GameObject.FindObjectsOfType<AudioSource>();

        for (int i = 0; i < sources.Length; i++)
        {
            AudioSource source = sources[i];

            if (!source.isPlaying)
                continue;
        }

        return null;
    }
    private float GetPercievedLoudness(Vector3 point, AudioSource source)
    {
        source.GetCustomCurve(AudioSourceCurveType.CustomRolloff);
        return 0f;
    }
}