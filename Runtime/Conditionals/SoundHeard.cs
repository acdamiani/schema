using System;
using UnityEngine;

namespace Schema.Builtin.Conditionals
{
    [Description(
         "Checks for playing AudioSources within a sphere. Note that these objects require a collider to be detected"),
     DarkIcon("Conditionals/d_SoundHeard"), LightIcon("Conditionals/SoundHeard")]
    public class SoundHeard : Conditional
    {
        [Tooltip("Radius of the sphere that is registered as \"hearing\" a noise")]
        public float radius = 60.0f;

        [Tooltip("The tags to filter from. Only these tags will be considered when listening for noise")]
        public TagFilter tagFilter;

        [Tooltip(
             "The entry to store the first heard GameObject to. When multiple audio sources are detected, it will store the closest one."),
         WriteOnly]
        public BlackboardEntrySelector<GameObject> heard;

        public override bool Evaluate(object decoratorMemory, SchemaAgent agent)
        {
            Collider[] colliders =
                Physics.OverlapSphere(agent.transform.position, radius, -1, QueryTriggerInteraction.UseGlobal);

            Tuple<AudioSource, float> closest = new Tuple<AudioSource, float>(null, float.MaxValue);

            foreach (Collider c in colliders)
            {
                if (!tagFilter.tags.Contains(c.tag))
                    continue;

                AudioSource source = c.GetComponent<AudioSource>();

                if (source == null || !source.isPlaying)
                    continue;

                Vector3 dir = c.transform.position - agent.transform.position;

                if (dir.sqrMagnitude < closest.Item2)
                    closest = new Tuple<AudioSource, float>(source, dir.sqrMagnitude);
            }

            heard.value = closest.Item1?.gameObject;

            return closest.Item1 != null;
        }
    }
}