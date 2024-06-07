using System.Collections.Generic;
using UnityEngine;

public class BeatGenerator : MonoBehaviour
{
    [System.Serializable]
    public class BeatPattern
    {
        public AudioClip beatClip;
        public AnimationCurve bpmCurve;
        public float startDelay; // Time delay before this beat pattern starts
        public bool endlessIncrease; // Option to turn endless increase on or off
    }

    public List<BeatPattern> beatPatterns;
    private List<AudioSource> audioSources = new List<AudioSource>();
    private float startTime;

    public CubeGenerator cubeGenerator; // Reference to the CubeGenerator

    void Start()
    {
        if (beatPatterns == null || beatPatterns.Count == 0)
        {
            Debug.LogError("No beat patterns assigned!");
            return;
        }

        if (cubeGenerator == null)
        {
            Debug.LogError("CubeGenerator not assigned!");
            return;
        }

        startTime = Time.time;

        foreach (var pattern in beatPatterns)
        {
            if (pattern.beatClip == null || pattern.bpmCurve == null)
            {
                Debug.LogError("Beat clip or BPM curve is missing in a pattern!");
                continue;
            }

            // Set the wrap mode of the animation curve to Clamp
            pattern.bpmCurve.postWrapMode = WrapMode.Clamp;

            // Create an AudioSource for each beat pattern
            AudioSource audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = pattern.beatClip;
            audioSource.spatialBlend = 1.0f; // 3D sound
            audioSource.maxDistance = 5000f; // Set a very large max distance
            audioSource.rolloffMode = AudioRolloffMode.Linear; // Linear rolloff
            audioSource.dopplerLevel = 0.0f; // Disable doppler effect
            audioSources.Add(audioSource);

            // Start the coroutine with the specified delay
            StartCoroutine(PlayBeatPattern(audioSource, pattern.bpmCurve, pattern.startDelay, pattern.endlessIncrease));
        }
    }

    System.Collections.IEnumerator PlayBeatPattern(AudioSource audioSource, AnimationCurve bpmCurve, float startDelay, bool endlessIncrease)
    {
        // Wait for the specified start delay
        yield return new WaitForSeconds(startDelay);

        while (true)
        {
            float elapsedTime = Time.time - startTime - startDelay;
            float currentBPM = bpmCurve.Evaluate(elapsedTime);

            if (endlessIncrease)
            {
                // Manually handle endless increase beyond the last keyframe
                Keyframe lastKey = bpmCurve[bpmCurve.length - 1];
                Keyframe secondLastKey = bpmCurve[bpmCurve.length - 2];
                float slope = (lastKey.value - secondLastKey.value) / (lastKey.time - secondLastKey.time);

                if (elapsedTime > lastKey.time)
                {
                    currentBPM = lastKey.value + slope * (elapsedTime - lastKey.time);
                }
            }

            float beatInterval = 60.0f / Mathf.Max(currentBPM, 0.1f); // Avoid division by zero

            // Calculate time for cube to fall to y = 0 也许这里要改
            ///////////////////////////////////////////////////////////////
            float fallTime = cubeGenerator.initialPosition.y / cubeGenerator.fallSpeed;

            // Trigger cube generation and synchronize beat
            StartCoroutine(GenerateCubeAndPlayBeat(audioSource, fallTime));

            yield return new WaitForSeconds(beatInterval);
        }
    }

    private System.Collections.IEnumerator GenerateCubeAndPlayBeat(AudioSource audioSource, float fallTime)
    {
        // Generate the cube
        cubeGenerator.GenerateCube();

        // Wait for the cube to fall to y = 0
        yield return new WaitForSeconds(fallTime);

        // Play the beat
        audioSource.Play();
    }
}
