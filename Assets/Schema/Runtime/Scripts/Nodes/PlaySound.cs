using UnityEngine;
using Schema;

[RequireAgentComponent(typeof(AudioSource))]
[DarkIcon("Dark/PlaySound")]
[LightIcon("Light/PlaySound")]
public class PlaySound : Action
{
    class PlaySoundMemory
    {
        public AudioSource source;
        public float start;
        public bool playingAudio;
    }

    [Tooltip("Whether to play a specific Audio Clip once. This is the most common use case,  as it does not require for a specifc Audio Clip to be attached to the Source")]
    public bool isOneShot = true;
    [Tooltip("The clip to play")]
    public AudioClip clip;
    [Tooltip("The volume to play the clip at")]
    [Range(0f, 1f)]
    public float volume = 1f;
    public bool waitForCompletion = false;
    public override void OnInitialize(object nodeMemory, SchemaAgent agent)
    {
        PlaySoundMemory memory = (PlaySoundMemory)nodeMemory;

        memory.source = agent.GetComponent<AudioSource>();
    }
    public override void OnNodeEnter(object nodeMemory, SchemaAgent agent)
    {
        PlaySoundMemory memory = (PlaySoundMemory)nodeMemory;

        memory.playingAudio = false;
    }
    public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
    {
        PlaySoundMemory memory = (PlaySoundMemory)nodeMemory;

        AudioClip clipToPlay = isOneShot ? clip : memory.source.clip;

        if (!memory.playingAudio)
        {
            memory.start = Time.time;

            if (isOneShot)
            {
                memory.source.PlayOneShot(clip, volume);
            }
            else
                memory.source.Play();

            memory.playingAudio = true;
        }

        if (waitForCompletion && Time.time - memory.start <= clipToPlay.length)
            return NodeStatus.Running;
        else
            return NodeStatus.Success;
    }
}