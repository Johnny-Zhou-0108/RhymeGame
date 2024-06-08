using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [System.Serializable]
    public class LevelParameters
    {
        public int minClipsToLoad = 2;
        public int maxClipsToLoad = 3;
        public float remixFrequency = 1.0f; // Lower value means faster frequency
        public float totalLength = 60.0f; // Total length of the remix in seconds
        public float initialDelay = 1.0f; // Initial delay from audiomanager
        public float perfectHitDistance = 0.2f; // Distance threshold for a perfect hit
        public float extraFallTime = 2.0f; // Extra fall time for missed cubes
        public int scoreThresholdForNextLevel = 100; // Score threshold to load next level
    }

    public List<LevelParameters> levels = new List<LevelParameters>();
    public AudioClip introAudioClip; // Audio clip to play in the intro scene

    private AudioSource audioSource;
    private int currentScore = 0;
    private int currentLevel = 0;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);

        // Add an AudioSource component if not already present
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Update()
    {
        // Check if the score threshold for the current level is met
        if (currentScore >= levels[currentLevel].scoreThresholdForNextLevel)
        {
            NextLevel();
        }

        // Temporary input check to stop remix and visuals
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            AudioManager audioManager = FindObjectOfType<AudioManager>();
            VisualManager visualManager = FindObjectOfType<VisualManager>();

            if (audioManager != null && visualManager != null)
            {
                audioManager.StopRemix();
                visualManager.StopVisuals();
                currentScore = 0;
            }
        }
    }

    public void PlayAudioAndLoadLevel()
    {
        if (introAudioClip != null)
        {
            // Play the audio clip
            audioSource.clip = introAudioClip;
            audioSource.Play();

            // Load the next scene after the audio clip finishes
            StartCoroutine(LoadLevelAfterAudio(introAudioClip.length));
        }
        else
        {
            Debug.LogError("Intro audio clip not assigned.");
        }
    }

    private IEnumerator LoadLevelAfterAudio(float delay)
    {
        yield return new WaitForSeconds(delay);

        // Load the Level 1 scene
        SceneManager.LoadScene("LV1");
        StartCoroutine(StartRemixAfterSceneLoad());
    }

    private IEnumerator StartRemixAfterSceneLoad()
    {
        // Wait for the scene to load completely
        yield return new WaitForSeconds(1.0f);

        // Find the AudioManager and VisualManager in the scene
        AudioManager audioManager = null;
        VisualManager visualManager = null;
        while (audioManager == null || visualManager == null)
        {
            audioManager = FindObjectOfType<AudioManager>();
            visualManager = FindObjectOfType<VisualManager>();
            yield return null;
        }

        StartLevel(audioManager, visualManager);
    }

    private void StartLevel(AudioManager audioManager, VisualManager visualManager)
    {
        // Set the parameters in the AudioManager and VisualManager based on the current level
        var levelParams = levels[currentLevel];
        audioManager.minClipsToLoad = levelParams.minClipsToLoad;
        audioManager.maxClipsToLoad = levelParams.maxClipsToLoad;
        audioManager.remixFrequency = levelParams.remixFrequency;
        audioManager.totalLength = levelParams.totalLength;
        audioManager.initialDelay = levelParams.initialDelay;

        visualManager.perfectHitDistance = levelParams.perfectHitDistance;
        visualManager.extraFallTime = levelParams.extraFallTime;

        // Reset the score
        currentScore = 0;

        // Start the remix and visuals
        audioManager.LoadAndStartRemix();
        visualManager.StartVisuals(audioManager.GetPlayScheduledTimes());
    }

    private void NextLevel()
    {
        if (currentLevel < levels.Count - 1)
        {
            currentLevel++;
            AudioManager audioManager = FindObjectOfType<AudioManager>();
            VisualManager visualManager = FindObjectOfType<VisualManager>();

            if (audioManager != null && visualManager != null)
            {
                // Stop current remix and visuals
                audioManager.StopRemix();
                visualManager.StopVisuals();
                Debug.Log("jjjjj");

                // Start the next level immediately
                StartLevel(audioManager, visualManager);
            }
            else
            {
                Debug.LogError("AudioManager or VisualManager not found in the scene.");
            }
        }
    }

    public void AddScore(int points)
    {
        currentScore += points;
        Debug.Log($"Current score: {currentScore}");
    }

    public int GetScore()
    {
        return currentScore;
    }
}
