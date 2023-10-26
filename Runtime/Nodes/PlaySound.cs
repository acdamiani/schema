using UnityEngine;

namespace Schema.Builtin.Nodes
{
    [DarkIcon("Nodes/d_Sound"), LightIcon("Nodes/Sound")]
    public class PlaySound : Action
    {
        public ComponentSelector<AudioSource> audioSource;

        [Tooltip(
            "Whether to play a specific Audio Clip once. This is the most common use case,  as it does not require for a specifc Audio Clip to be attached to the Source")]
        public bool isOneShot = true;

        [Tooltip("The clip to play")] public AudioClip clip;

        [Tooltip("The volume to play the clip at"), Range(0f, 1f)] 
        public float volume = 1f;

        public bool waitForCompletion;

        public override NodeStatus Tick(object nodeMemory, SchemaAgent agent)
        {
            PlaySoundMemory memory = (PlaySoundMemory)nodeMemory;

            AudioSource a = agent.GetComponent(audioSource);

            if (a == null)
                return NodeStatus.Failure;

            AudioClip clipToPlay = isOneShot ? clip : a.clip;

            if (!memory.playingAudio)
            {
                memory.start = Time.time;

                if (isOneShot)
                    a.PlayOneShot(clip, volume);
                else
                    a.Play();

                memory.playingAudio = true;
            }

            if (waitForCompletion && Time.time - memory.start <= clipToPlay.length)
                return NodeStatus.Running;
            return NodeStatus.Success;
        }

        private class PlaySoundMemory
        {
            public bool playingAudio;
            public float start;
        }
    }
}