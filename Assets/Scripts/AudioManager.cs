using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public string audioFolderPath = "AudioFiles"; // The relative path to the folder within Resources
    public int minClipsToLoad = 2;
    public int maxClipsToLoad = 3;
    public float remixFrequency = 1.0f; // Lower value means faster frequency
    public float totalLength = 60.0f; // Total length of the remix in seconds
    public float initialDelay = 1.0f; // Initial delay before starting the remix
    public AudioSource audioSourcePrefab; // The AudioSource prefab to create instances from

    private AudioClip[] loadedClips;
    public List<double> playScheduledTimes = new List<double>(); // List to store the PlayScheduled times
    private List<AudioSource> audioSourcePool = new List<AudioSource>(); // List to store pooled AudioSource instances
    private Coroutine currentRemixCoroutine;
    private int poolSize = 10; // Adjust the pool size based on your needs

    void Start()
    {
        // Initialize the AudioSource pool
        for (int i = 0; i < poolSize; i++)
        {
            AudioSource newSource = Instantiate(audioSourcePrefab, transform);
            newSource.gameObject.SetActive(false);
            audioSourcePool.Add(newSource);
        }
    }

    public void LoadAndStartRemix()
    {
        StopCurrentRemix(); // Stop the current remix if it's running
        LoadRandomAudioClips(); // Call this method to load random audio clips from the specified folder
        if (loadedClips != null && loadedClips.Length > 0)
        {
            GeneratePlayScheduledTimes();
            currentRemixCoroutine = StartCoroutine(RemixAudio()); // Start the remix coroutine
        }
        else
        {
            Debug.LogError("No audio clips were loaded. Please check the audio folder path and ensure it contains audio files.");
        }
    }

    public void StopRemix()
    {
        StopCurrentRemix(); // Stop the current remix if it's running
    }

    void StopCurrentRemix()
    {
        if (currentRemixCoroutine != null)
        {
            StopCoroutine(currentRemixCoroutine);
            currentRemixCoroutine = null;
        }

        // Stop and deactivate all audio sources
        foreach (var source in audioSourcePool)
        {
            if (source != null)
            {
                source.Stop();
                source.gameObject.SetActive(false);
            }
        }
    }

    void LoadRandomAudioClips()
    {
        AudioClip[] allClips = Resources.LoadAll<AudioClip>(audioFolderPath);
        Debug.Log("Found " + allClips.Length + " clips in the specified folder: " + audioFolderPath);

        if (allClips.Length == 0)
        {
            Debug.LogError("No audio clips found in the specified folder.");
            return;
        }

        int clipsToLoad = Mathf.Min(Random.Range(minClipsToLoad, maxClipsToLoad + 1), allClips.Length);
        loadedClips = new AudioClip[clipsToLoad];

        for (int i = 0; i < clipsToLoad; i++)
        {
            loadedClips[i] = allClips[Random.Range(0, allClips.Length)];
            Debug.Log("Loaded clip: " + loadedClips[i].name);
        }
    }

    void GeneratePlayScheduledTimes()
    {
        double startTime = AudioSettings.dspTime + initialDelay;
        double currentTime = startTime;

        playScheduledTimes.Clear(); // Clear previous scheduled times

        while (currentTime - startTime < totalLength)
        {
            playScheduledTimes.Add(currentTime);
            currentTime += remixFrequency;
        }

        Debug.Log("Generated play scheduled times: [" + string.Join(", ", playScheduledTimes) + "]");
    }

    IEnumerator RemixAudio()
    {
        foreach (var playTime in playScheduledTimes)
        {
            AudioClip clipToPlay = loadedClips[Random.Range(0, loadedClips.Length)];
            if (clipToPlay == null)
            {
                Debug.LogError("Clip to play is null.");
                continue;
            }

            // Get an available AudioSource from the pool
            AudioSource source = GetAvailableAudioSource();
            if (source == null)
            {
                Debug.LogError("No available AudioSource in the pool.");
                continue;
            }

            source.clip = clipToPlay;
            source.gameObject.SetActive(true);
            source.PlayScheduled(playTime);
            Debug.Log("Scheduled to play clip: " + clipToPlay.name + " at time: " + playTime);

            // Ensure there's enough time between scheduled plays
            yield return new WaitForSeconds(remixFrequency);
        }
    }

    AudioSource GetAvailableAudioSource()
    {
        foreach (var source in audioSourcePool)
        {
            if (!source.isPlaying)
            {
                return source;
            }
        }
        return null;
    }

    // Method to get the PlayScheduled times array
    public double[] GetPlayScheduledTimes()
    {
        return playScheduledTimes.ToArray();
    }
}
