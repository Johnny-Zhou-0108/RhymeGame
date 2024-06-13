using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

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
    public AudioClip endAudioClip; // Audio clip to play at the end of the last level
    public AudioClip perfectStreakAudioClip; // Audio clip to play on perfect streak
    public Button beginButton;

    public int perfectHitStreakCount = 5; // Number of consecutive perfect hits required
    private int consecutivePerfectHits = 0; // Track the number of consecutive perfect hits
    private bool perfectStreakAchieved = false; // Flag to track if the streak has been achieved

    private AudioSource audioSource;
    private AudioSource endAudioSource;
    private int currentScore = 0;
    private int currentLevel = 0;
    private bool endAudioPlayed = false;
    private bool isRemixFinished = false; // Flag to track if the remix is finished

    void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Make the instance persistent across scenes
        }
        else
        {
            Destroy(gameObject);
        }

        // Add an AudioSource component if not already present
        audioSource = gameObject.AddComponent<AudioSource>();
        endAudioSource = gameObject.AddComponent<AudioSource>();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Intro")
        {
            currentScore = 0;
            beginButton = GameObject.Find("BeginButton").GetComponent<Button>();

            if (beginButton != null)
            {
                //Debug.Log("Begin button reassigned");
                // Add any additional logic for the button here
                beginButton.onClick.AddListener(OnBeginButtonClicked);
            }
        }
    }

    void Update()
    {
        // Check if the score threshold for the current level is met
        if (currentScore >= levels[currentLevel].scoreThresholdForNextLevel)
        {
            if (currentLevel < levels.Count - 1)
            {
                NextLevel();
            }
            else if (!endAudioPlayed)
            {
                PlayEndAudio();
                endAudioPlayed = true; // Set the flag to prevent multiple triggers
            }
        }

        // Temporary input check to stop remix and visuals
        

        // Check if the remix is finished and the score threshold is not met
        AudioManager audioManagerCheck = FindObjectOfType<AudioManager>();
        if (audioManagerCheck != null && audioManagerCheck.IsRemixFinished() && !isRemixFinished)
        {
            isRemixFinished = true;
            if (currentScore < levels[currentLevel].scoreThresholdForNextLevel)
            {
                RestartRemixAndVisuals();
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

        // Reset the score and flags
        currentScore = 0;
        endAudioPlayed = false; // Reset the end audio flag
        isRemixFinished = false; // Reset the remix finished flag
        consecutivePerfectHits = 0; // Reset the perfect hits count
        perfectStreakAchieved = false; // Reset the streak achieved flag

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

                // Start the next level immediately
                StartLevel(audioManager, visualManager);
            }
            else
            {
                Debug.LogError("AudioManager or VisualManager not found in the scene.");
            }
        }
    }

    private void PlayEndAudio()
    {
        AudioManager audioManager = FindObjectOfType<AudioManager>();
        VisualManager visualManager = FindObjectOfType<VisualManager>();

        if (audioManager != null && visualManager != null)
        {
            // Stop current visuals
            // mentioned in the voice over, but I still choose to stop the visuals since they are too distracting
            visualManager.StopVisuals();
            // to play the end audio
            InputManager.Instance.LastClip = true;
            // to prevent the reminder clip to be played
            InputManager.Instance.isOtherClipPlaying = true;

            if (endAudioSource != null && endAudioClip != null)
            {
                endAudioSource.clip = endAudioClip;
                endAudioSource.loop = false;
                endAudioSource.Play();
                //Debug.Log("Playing end audio clip");
            }
            else
            {
                //Debug.LogError("EndAudioSource or EndAudioClip is not assigned.");
            }
        }
        else
        {
            //Debug.LogError("AudioManager or VisualManager not found in the scene.");
        }
    }

    public void AddScore(int points)
    {
        currentScore += points;
        VisualManager visualManager = FindObjectOfType<VisualManager>();
        if (visualManager != null)
        {
            visualManager.UpdateScoreText(currentScore);
        }

        if (points > 0) // Assuming perfect hit gives positive points
        {
            consecutivePerfectHits++;
            if (consecutivePerfectHits >= perfectHitStreakCount && !perfectStreakAchieved)
            {
                PlayPerfectStreakAudio();
                perfectStreakAchieved = true;
            }
        }
        else
        {
            consecutivePerfectHits = 0; // Reset streak on miss
        }

        //Debug.Log($"Current score: {currentScore}");
    }

    public int GetScore()
    {
        return currentScore;
    }

    private void RestartRemixAndVisuals()
    {
        AudioManager audioManager = FindObjectOfType<AudioManager>();
        VisualManager visualManager = FindObjectOfType<VisualManager>();

        if (audioManager != null && visualManager != null)
        {
            Debug.Log("Restarting remix and visuals for the current level.");
            StartLevel(audioManager, visualManager);
        }
        else
        {
            Debug.LogError("AudioManager or VisualManager not found in the scene.");
        }
    }

    private void PlayPerfectStreakAudio()
    {
        if (perfectStreakAudioClip != null)
        {
            audioSource.PlayOneShot(perfectStreakAudioClip);
            Debug.Log("Playing perfect streak audio clip");
        }
    }

    public void OnBeginButtonClicked()
    {
        if (beginButton != null)
        {
            //beginButton.gameObject.SetActive(false);
            beginButton.interactable = false;
            Image buttonImage = beginButton.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.enabled = false;
            }

            // If there are any Text or child Images under the button, disable them too
            foreach (var graphic in beginButton.GetComponentsInChildren<Graphic>())
            {
                graphic.enabled = false;
            }
        }
        PlayAudioAndLoadLevel();
    }
}
