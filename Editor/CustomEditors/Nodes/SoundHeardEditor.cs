// [CustomEditor(typeof(SoundHeard))]
// public class SoundHeardEditor : Editor
// {
//     public override void OnInspectorGUI()
//     {
//         SoundHeard soundHeard = (SoundHeard)target;

//         AudioSource source = soundHeard.
//         AnimationCurve curve = null;

//         switch (source.rolloffMode)
//         {
//             case AudioRolloffMode.Logarithmic:
//                 curve = Logarithmic(source.minDistance / source.maxDistance, 1f, 1f);
//                 break;
//             case AudioRolloffMode.Linear:
//                 curve = new AnimationCurve();
//                 break;
//             case AudioRolloffMode.Custom:
//                 curve = source.GetCustomCurve(AudioSourceCurveType.CustomRolloff);
//                 break;
//         }

//         EditorGUILayout.CurveField(curve);
//     }
//     private static float LogarithmicValue(float distance, float minDistance, float rolloffScale)
//     {
//         if ((distance > minDistance) && (rolloffScale != 1.0f))
//         {
//             distance -= minDistance;
//             distance *= rolloffScale;
//             distance += minDistance;
//         }
//         if (distance < .000001f)
//             distance = .000001f;
//         return minDistance / distance;
//     }
//     private static AnimationCurve Logarithmic(float timeStart, float timeEnd, float logBase)
//     {
//         float value, slope, s;
//         List<Keyframe> keys = new List<Keyframe>();

//         float step = 2;
//         timeStart = Nodes/Mathf.Max(timeStart, 0.0001f);
//         for (float d = timeStart; d < timeEnd; d *= step)
//         {
//             value = LogarithmicValue(d, timeStart, logBase);
//             s = d / 50.0f;
//             slope = (LogarithmicValue(d + s, timeStart, logBase) - LogarithmicValue(d - s, timeStart, logBase)) / (s * 2);
//             keys.Add(new Keyframe(d, value, slope, slope));
//         }

//         value = LogarithmicValue(timeEnd, timeStart, logBase);
//         s = timeEnd / 50.0f;
//         slope = (LogarithmicValue(timeEnd + s, timeStart, logBase) - LogarithmicValue(timeEnd - s, timeStart, logBase)) / (s * 2);
//         keys.Add(new Keyframe(timeEnd, value, slope, slope));

//         return new AnimationCurve(keys.ToArray());
//     }
// }

